// -----------------------------------------------------------------------
// <copyright file="MetersToBillingJobStage.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Security.KeyVault.Secrets;
using LogAnalytics.Client;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.BillingServiceClient;
using Microsoft.Purview.DataGovernance.BillingServiceClient.Entities;
using Microsoft.Purview.DataGovernance.BillingServiceClient.Entities.BillingEventParameters;
using Microsoft.Purview.DataGovernance.BillingServiceClient.Helper;
using Microsoft.Purview.DataGovernance.Common;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;
using Polly;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class MetersToBillingJobStage : IJobCallbackStage
{
    private readonly MetersToBillingJobMetadata metadata;
    private readonly JobCallbackUtils<MetersToBillingJobMetadata> jobCallbackUtils;

    private readonly IServiceScope scope;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IOptionsMonitor<AuditServiceConfiguration> auditServiceConfiguration;
    private readonly IBillingServiceClient billingServiceClient;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;
    private readonly AzureCredentialFactory credentialFactory;
    private readonly LogAnalyticsManager.LogAnalyticsQueryClient logsAnalyticsReader;
    private readonly LogAnalyticsClient logsAnalyticsWriter;

    private string workspaceId;
    private string workspaceKey;


    internal MetersToBillingJobStage(
        IServiceScope scope,
        MetersToBillingJobMetadata metadata,
        JobCallbackUtils<MetersToBillingJobMetadata> jobCallbackUtils)
    {

        this.scope = scope;
        this.metadata = metadata;
        this.credentialFactory = scope.ServiceProvider.GetService<AzureCredentialFactory>();
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.billingServiceClient = scope.ServiceProvider.GetService<IBillingServiceClient>();
        this.auditServiceConfiguration = scope.ServiceProvider.GetService<IOptionsMonitor<AuditServiceConfiguration>>();
        this.jobCallbackUtils = jobCallbackUtils;
        this.keyVaultAccessorService = scope.ServiceProvider.GetService<IKeyVaultAccessorService>();
        var keyVaultConfig = scope.ServiceProvider.GetService<IOptions<KeyVaultConfiguration>>();
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();


        this.logger.LogInformation("MetersToBillingJobStage Constructor.1");

        // Get logAnalyticsWriter credentials
        var task = Task.Run(async () =>
        {
            await this.GetWorkspaceCredentials();
        });
        Task.WaitAll(task);

        this.logsAnalyticsWriter = new LogAnalyticsClient(workspaceId: this.workspaceId, sharedKey: this.workspaceKey);

        // Log analytics reader
        LogAnalyticsManager manager = new LogAnalyticsManager(this.credentialFactory.CreateDefaultAzureCredential());
        this.logsAnalyticsReader = manager.WithWorkspace(this.workspaceId);


        this.logger.LogInformation("MetersToBillingJobStage Finished");

    }

    public string StageName => nameof(MetersToBillingJobStage);

    public bool IsStagePreconditionMet()
    {
        return true;
    }

    public async Task<JobExecutionResult> Execute()
    {
        this.logger.LogInformation("Starting Metering Job." + this.metadata.TenantId);

        // job final state initializaation
        var jobStarted = DateTimeOffset.UtcNow;
        string finalExecutionStatusDetails = $"Job Started at {jobStarted.ToString()}";
        var finalExecutionStatus = JobExecutionStatus.Faulted;
        this.logger.LogInformation(finalExecutionStatusDetails);


        ////////////////////////////////////
        // DEH Processing

        finalExecutionStatusDetails += await this.ProcessBilling("PDG_deh_billing.kql");
        finalExecutionStatusDetails += await this.ProcessBilling("PDG_dq_billing.kql");

        finalExecutionStatus = JobExecutionStatus.Succeeded;

        return this.jobCallbackUtils.GetExecutionResult(
              finalExecutionStatus,
              finalExecutionStatusDetails);
    }

    private async Task<string> ProcessBilling(string kql)
    {
        string finalStatus = string.Empty;
        DateTimeOffset started = DateTimeOffset.UtcNow;
        try
        {
            int totalEvents = await this.ProcessBillingEvents(kql);
            // set the final details into the job
            finalStatus = $" | KQL: {kql} | Completed on {DateTimeOffset.Now.ToString()} | Duration {(DateTimeOffset.UtcNow - started).TotalSeconds} seconds. | Total Events Processed: {totalEvents}";
        }
        catch (Exception ex)
        {
            // set the final details when faulted
            finalStatus += $" | KQL: {kql} | Failed on {DateTimeOffset.Now.ToString()} | Duration {(DateTimeOffset.UtcNow - started).TotalSeconds} seconds. Reason: {ex.ToString()}";
            this.logger.LogError(finalStatus, ex);
        }

        return finalStatus;
    }

    private async Task<int> ProcessBillingEvents(string DEHBillingProcesingKQL)
    {
        // Reprocessing window is last 7 days
        DateTimeOffset pollFrom = DateTimeOffset.UtcNow.AddDays(-7);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        // Query for Meters:
        // pwdg_billing_deh (or any other function involved in billing) must join CBSBillingReceipts_CL by using EvendId_g and
        // only emit those billable events not having a CBSBillingReceipts_CL.Status_s  == "Succeded" 
        // 
        // This approach ensures auto-healing/auto-retry of all billable events for the last 7 days.        

        var billingReceipts = "CBSBillingReceipts_CL";
        try
        {
            var meteredEvent = await this.logsAnalyticsReader.Query<CBSBillingReceipt>(billingReceipts, pollFrom, utcNow);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error in Querying {billingReceipts}, Table not found", ex);

            List<CBSBillingReceipt> billingEvents = new List<CBSBillingReceipt>();
            CBSBillingReceipt cBSBillingReceipt = new CBSBillingReceipt()
            {
                BillingTimestamp = DateTime.Now,
                BillingEvent = "coldStart",
                CorrelationId = Guid.NewGuid().ToString(),
                EventId = Guid.NewGuid().ToString(),
                BatchId = Guid.NewGuid().ToString(),
                Service = "NA",
                Status = CBSReceiptStatus.Unknown,
                StatusDetail = String.Empty
            };

            billingEvents.Add(cBSBillingReceipt);
            var coldStartReceipts = billingEvents.ToList();

            //Create the table if it does not exist
            await this.logsAnalyticsWriter.SendLogEntries<CBSBillingReceipt>(coldStartReceipts, billingReceipts);

        }
        var meteredEvents = await this.logsAnalyticsReader.Query<DEHMeteredEvent>(await this.LoadKQL(DEHBillingProcesingKQL), pollFrom, utcNow);

        int totalEvents = 0;

        // if we get any data
        if (meteredEvents != null && meteredEvents.Value != null && meteredEvents.Value.Count > 0)
        {
            // update from last time poll
            this.metadata.LastPollTime = utcNow.ToString();

            totalEvents = meteredEvents.Value.Count;

            var maxRetries = 6;

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(maxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        this.logger.LogInformation($"{this.StageName} | Retry {retryCount} out of {maxRetries} because of: {exception.Message}. Waiting {timeSpan} before next retry.");
                    });

            // push events

            var useBatches = true;
            var batchSize = 1;
            var currentBatch = 0;

            if (useBatches)
            {
                batchSize = 25;
            }

            // consume all batches
            while (true)
            {
                var batch = meteredEvents.Value.Skip(currentBatch * batchSize).Take(batchSize).ToList().Select(meteredEvent =>
                {
                    var billingTags = new BillingTags
                    {
                        AccountId = Guid.Parse(meteredEvent.AccountId)
                    };

                    var now = DateTime.UtcNow;

                    var billingEvent = BillingEventHelper.CreateProcessingUnitBillingEvent(new ProcessingUnitBillingEventParameters
                    {
                        EventId = Guid.Parse(meteredEvent.JobId), // EventId is use for dedup downstream - handle with care
                        TenantId = Guid.Parse(meteredEvent.AccountId),
                        CreationTime = now,
                        // BUG IN CBS DO NOT ALLOW FRACTIONAL UNITS
                        Quantity = meteredEvent.ProcessingUnits,
                        BillingTags = billingTags                        
                    });

                    return billingEvent;

                }).ToList();

                if (batch.Count > 0)
                {
                    await this.EmitEventsWithRetryAndErrorHandling($"Batch number {currentBatch}. Batch Size {batchSize}. Emitted so far.... {(currentBatch * batchSize) + batchSize} billable events",
                                            batch, Guid.NewGuid());
                    // next batch
                    currentBatch++;
                }
                else
                {
                    // no more data.
                    break;
                }
            }
        }
        return totalEvents;
    }

    private async Task GetWorkspaceCredentials()
    {

        KeyVaultSecret workspaceId = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", default(CancellationToken));
        KeyVaultSecret workspaceKey = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsKey", default(CancellationToken));

        this.workspaceId = workspaceId.Value;
        this.workspaceKey = workspaceKey.Value;
    }

    private async Task<string> LoadKQL(string queryName)
    {
        string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        string[] files = Directory.GetFiles(currentDirectory, queryName, SearchOption.AllDirectories);

        string filePath = "";
        if (files.Length > 0)
        {
            filePath = files[0];
        }

        this.logger.LogInformation("KQL Folder Path: " + filePath);

        string scriptText = "";
        using (this.logger.LogElapsed("Start Loading the script"))
        {
            this.logger.LogInformation(Path.Combine(currentDirectory, queryName));
            scriptText = await File.ReadAllTextAsync(Path.Combine(currentDirectory, queryName));
        }

        this.logger.LogInformation("KQL script: " + queryName + scriptText);
        return scriptText;
    }

    private async Task EmitEventsWithRetryAndErrorHandling(string logShared, List<BillingEvent> billingEvents, Guid correlationId)
    {
        try
        {
            this.logger.LogInformation($"Emitting billingEvents -> {logShared}");

            foreach (var item in billingEvents)
            {
                this.logger.LogInformation($"Emitting billing event: {item.ToJson()}");
            }

            var response = await this.billingServiceClient.SubmitBillingEventsAsync(billingEvents, correlationId);

            // prepopulate all the CBS Receipts (optimistically succeded)
            var receipts = billingEvents.Select(q =>
            {
                return new CBSBillingReceipt()
                {
                    BillingTimestamp = q.CreationTime,
                    BillingEvent = JsonConvert.SerializeObject(q),
                    CorrelationId = q.CorrelationId.ToString(),
                    EventId = q.Id.ToString(),
                    BatchId = correlationId.ToString(),
                    Service = q.SolutionName,
                    Status = CBSReceiptStatus.Unknown,
                    StatusDetail = String.Empty
                };
            }).ToList();

            // https://microsoft.sharepoint.com/teams/AuditingOnboarding/SitePages/Start-Sending-the-Audit-Data.aspx#http-response-error-type
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        // Batch succeded completely.
                        foreach (var receipt in receipts)
                        {
                            receipt.Status = CBSReceiptStatus.Succeded;
                            receipt.StatusDetail = JsonConvert.SerializeObject(response);
                        }
                    }
                    break;
                case HttpStatusCode.PartialContent:

                    {
                        // for Partial Content some of the batch entries failed with validation errors.
                        // Process the errors, find the correlated id and populate the failed reasons.
                        //
                        // if we have any problem with correlation we fail the whole batch for further inspection
                        //

                        var failWholeBatch = true;
                        var globalFailure = "Response Content or response.ContentValidationErrors is null";

                        if (response.Content != null && response.Content.ValidationErrors != null)
                        {
                            try
                            {
                                // 1. set all ok
                                foreach (var receipt in receipts)
                                {
                                    receipt.Status = CBSReceiptStatus.Succeded;
                                    receipt.StatusDetail = String.Empty;
                                }

                                // 2. override the status for those failed
                                foreach (var validationError in response.Content.ValidationErrors)
                                {
                                    var validatedBillinvalidationErrorgEvent = validationError;
                                    var data = JsonConvert.DeserializeObject<BillingEvent>(validationError.JsonData);


                                    // correlate
                                    var receipt = receipts.Where(q => q.EventId == data.Id.ToString()).FirstOrDefault();

                                    if (receipt != null)
                                    {
                                        receipt.Status = CBSReceiptStatus.Failed;
                                        receipt.StatusDetail = JsonConvert.SerializeObject(validationError);
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException($"Can't correlate: {data.Id.ToString()} from {JsonConvert.SerializeObject(validationError)}");
                                    }
                                }

                                // correlated all the errors with the batch, batch succesfully processed
                                failWholeBatch = false;
                            }
                            catch (Exception ex)
                            {
                                failWholeBatch = true;
                                globalFailure = ex.ToString();
                            }
                        }

                        if (failWholeBatch)
                        {
                            foreach (var receipt in receipts)
                            {
                                receipt.Status = CBSReceiptStatus.BatchFailure;
                                receipt.StatusDetail = globalFailure;
                            }
                        }
                    }
                    break;
                default:
                    foreach (var receipt in receipts)
                    {
                        receipt.Status = CBSReceiptStatus.UnknownUnsuccesfulHttpCode;
                        receipt.StatusDetail = JsonConvert.SerializeObject(response);
                    }
                    break;

            }

            // Save Receipts
            await this.logsAnalyticsWriter.SendLogEntries<CBSBillingReceipt>(receipts, "CBSBillingReceipts");

            foreach (var item in receipts)
            {
                this.logger.LogInformation($"Emitting CBS Receipt: {item.ToJson()}");
            }

        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError($"Emit FAILED (HttpRequestException: {ex.ToString()}) | {logShared}");
            // The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
            // TODO: retry & log
            throw;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"Emit FAILED (InvalidOperationException: {ex.ToString()}) | {logShared}");
            // The requestUri must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set.
            // TODO: retry & log
            throw;
        }
        catch (TaskCanceledException ex)
        {
            this.logger.LogError($"Emit FAILED (TaskCanceledException: {ex.ToString()}) | {logShared}");
            // The requestUri must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set.
            // TODO: retry & log
            // .NET Core and .NET 5.0 and later only: The request failed due to timeout.
            // TODO: retry & log
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Emit FAILED (Exception ex: {ex.ToString()}) -> {logShared}");
            // The requestUri must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set.
            // TODO: retry & log
            // .NET Core and .NET 5.0 and later only: The request failed due to timeout.
            // TODO: retry & log
            throw;
        }

        this.logger.LogInformation($"Finished | {logShared}");
    }

    public bool IsStageComplete()
    {
        // we want this stage to run alwayas as it is was not completed
        return false;
    }
}
