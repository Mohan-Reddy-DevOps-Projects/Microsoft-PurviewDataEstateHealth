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
    public enum QueryType
    {
        Deh,
        Dq,
        Govern
    }

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

        using (this.logger.LogElapsed("MetersToBillingJobStage Constructor"))
        {
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
            //this.logger.LogInformation("MetersToBillingJobStage Finished");
        }
    }

    public string StageName => nameof(MetersToBillingJobStage);

    public bool IsStagePreconditionMet()
    {
        return true;
    }

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"{this.GetType().Name}: Starting Metering Job." + this.metadata.TenantId))
        {
            // job final state initializaation
            var jobStarted = DateTimeOffset.UtcNow;
            string finalExecutionStatusDetails = $"Job Started at {jobStarted.ToString()}";
            var finalExecutionStatus = JobExecutionStatus.Faulted;
            this.logger.LogInformation(finalExecutionStatusDetails);

            // Create tables if not exist
            await this.InitializeTables();

            ////////////////////////////////////
            // DEH Processing
            // Reprocessing window is last 7 days

            DateTimeOffset pollFrom = DateTimeOffset.UtcNow.AddDays(-7);
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            finalExecutionStatusDetails += await this.ProcessBilling<DEHMeteredEvent>("PDG_deh_billingV1.kql", QueryType.Deh, pollFrom, utcNow);
            finalExecutionStatusDetails += await this.ProcessBilling<DEHMeteredEvent>("PDG_dq_billing.kql", QueryType.Dq, pollFrom, utcNow);
            //finalExecutionStatusDetails += await this.ProcessBilling<GovernedAssetsMeteredEvent>("PDG_governed_assets_billing.kql", QueryType.Govern, pollFrom, utcNow);

            finalExecutionStatus = JobExecutionStatus.Succeeded;

            return this.jobCallbackUtils.GetExecutionResult(
                  finalExecutionStatus,
                  finalExecutionStatusDetails);
        }
    }

    private async Task CreateTableIfNotExists<T>(string tableName, Func<T> createDefaultItem) where T : class
    {
        try
        {
            await this.logsAnalyticsReader.Query<T>(tableName, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow);
            this.logger.LogInformation($"{tableName} table found.");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Error querying {tableName}: Table not found.", ex);

            var defaultItem = createDefaultItem();
            await this.logsAnalyticsWriter.SendLogEntries(new List<T> { defaultItem }, tableName);
            this.logger.LogInformation($"Created {tableName} table.");
        }
    }
    private async Task InitializeTables()
    {
        await this.CreateTableIfNotExists<DEHProcessedJobs>("DEHProcessedJobs_CL", () => new DEHProcessedJobs
        {
            //JobStartTime = DateTime.Now,
            //JobEndTime = DateTime.Now,
            TenantId = "coldStart",
            AccountId = Guid.NewGuid().ToString(),
            MDQBatchId = Guid.NewGuid().ToString(),
            JobId = Guid.NewGuid().ToString(),
            MDQJobDuration = 0,
            MDQProcessingUnits = 0,
            DEHJobDuration = 0,
            DEHProcessingUnits = 0
        });
    }

    private async Task LogProcessedJobs(string kql, DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        var started = DateTimeOffset.UtcNow;
        var table = "DEHProcessedJobs_CL";
        try
        {
            var dehJobs = await this.logsAnalyticsReader.Query<DEHProcessedJobs>(await this.LoadKQL(kql), fromDate, toDate);
            if (dehJobs.Value.Any())
            {
                var t = dehJobs.Value.ToList();
                await this.logsAnalyticsWriter.SendLogEntries<DEHProcessedJobs>(dehJobs.Value.ToList(), table);
            }
        }
        catch (Exception ex)
        {
            var duration = (DateTimeOffset.UtcNow - started).TotalSeconds;
            var errorMessage = ex != null ? $" Reason: {ex}" : string.Empty;
            var finalStatus = $" | KQL: {kql} | {"Failed"} on {DateTimeOffset.Now} | Duration {duration} seconds. | {errorMessage}";
            this.logger.LogError(finalStatus);
        }
    }

    private async Task<string> ProcessBilling<T>(string kql, QueryType queryType, DateTimeOffset fromDate, DateTimeOffset toDate) where T : MeteredEvent
    {
        string finalStatus = string.Empty;
        DateTimeOffset started = DateTimeOffset.UtcNow;
        using (this.logger.LogElapsed($"{this.GetType().Name}: {this.StageName} | Processing Billing with KQL: {kql}"))
        {
            try
            {
                int totalEvents = await this.ProcessBillingEvents<T>(kql, fromDate, toDate);
                if (queryType == QueryType.Deh)
                {
                    await this.LogProcessedJobs("PDG_deh_jobs.kql", fromDate, toDate);
                }
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
    }

    private async Task<int> ProcessBillingEvents<T>(string DEHBillingProcesingKQL, DateTimeOffset fromDate, DateTimeOffset toDate) where T : MeteredEvent
    {
        // Query for Meters:
        // pwdg_billing_deh (or any other function involved in billing) must join CBSBillingReceipts_CL by using EvendId_g and
        // only emit those billable events not having a CBSBillingReceipts_CL.Status_s  == "Succeded" 
        // 
        // This approach ensures auto-healing/auto-retry of all billable events for the last 7 days.        

        var billingReceipts = "CBSBillingReceipts_CL";
        try
        {
            var meteredEvent = await this.logsAnalyticsReader.Query<CBSBillingReceipt>(billingReceipts, fromDate, toDate);
            this.logger.LogInformation($"{this.GetType().Name}:|{billingReceipts} table found in Log Analytics.");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{this.GetType().Name}:|Error in Querying {billingReceipts}, Table not found", ex);

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
            this.logger.LogInformation($"{this.GetType().Name}:|Created {billingReceipts} table.");

        }

        var meteredEvents = await this.logsAnalyticsReader.Query<T>(await this.LoadKQL(DEHBillingProcesingKQL), fromDate, toDate);
        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Querying {DEHBillingProcesingKQL} from {fromDate} to {toDate}.");
        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Querying {JsonConvert.SerializeObject(meteredEvents)}");

        int totalEvents = 0;

        // if we get any data
        if (meteredEvents != null && meteredEvents.Value != null && meteredEvents.Value.Count > 0)
        {
            this.logger.LogInformation($"{this.GetType().Name}:|{meteredEvents.Value.Count.ToString()}");
            // update from last time poll
            this.metadata.LastPollTime = toDate.ToString();

            totalEvents = meteredEvents.Value.Count;

            var maxRetries = 6;

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(maxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Retry {retryCount} out of {maxRetries} because of: {exception.Message}. Waiting {timeSpan} before next retry.");
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
                var batch = meteredEvents.Value.Skip(currentBatch * batchSize).Take(batchSize).ToList().Where(meteredEvent => meteredEvent != null).Select(meteredEvent =>
                {
                    //var billingTags = new BillingTags
                    //{
                    //    AccountId = Guid.Parse(meteredEvent.AccountId)
                    //};

                    Dictionary<string, string> scopeMappings = new Dictionary<string, string>
                    {
                        { "DEH", "DEH_Controls" },
                        { "BYOC", "DEH_BYOC" },
                        { "DQ", "DQ" }
                    };

                    string scopeName = scopeMappings.TryGetValue(meteredEvent.DMSScope.ToUpperInvariant(), out var mappedName) ? mappedName : "Unknown";
                    var now = DateTime.UtcNow;

                    ExtendedBillingEvent billingEvent = null;
                    this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | meteredEvent: {JsonConvert.SerializeObject(meteredEvent)}");

                    if (meteredEvent is DEHMeteredEvent dehMeteredEvent)
                    {
                        Guid jobIdGuid = new Guid();
                        Guid tenantId = new Guid();

                        if (meteredEvent.DMSScope.ToUpperInvariant() == "DEH")
                        {
                            var billingTags = $"{{\"AccountId\":\"{meteredEvent.AccountId}\",";
                            billingTags += $"\"TenantId\":\"{meteredEvent.TenantId}\",";
                            billingTags += $"\"ConsumedUnit\":\"Data Management Processing Unit\",";
                            billingTags += $"\"SKU\":\"{this.getProcessSKU(dehMeteredEvent.ProcessingTier)}\",";
                            billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | billingTags: {billingTags}");

                            billingEvent = BillingEventHelper.CreateProcessingUnitBillingEvent(new ProcessingUnitBillingEventParameters
                            {
                                //Guid.Parse(dehMeteredEvent.MDQBatchId), // EventId is use for dedup downstream - handle with care
                                EventId = Guid.Parse(dehMeteredEvent.MDQBatchId),
                                TenantId = Guid.Parse(dehMeteredEvent.TenantId),
                                CreationTime = now,
                                Quantity = dehMeteredEvent.ProcessingUnits,
                                BillingTags = billingTags,
                                BillingStartDate = dehMeteredEvent.JobStartTime.DateTime,
                                BillingEndDate = dehMeteredEvent.JobEndTime.DateTime,
                                SKU = this.getProcessSKU(dehMeteredEvent.ProcessingTier),
                                LogOnly = false
                            });
                        }
                        else if (meteredEvent.DMSScope.ToUpperInvariant() == "DQ" && Guid.TryParse(dehMeteredEvent.JobId, out jobIdGuid) &&
                                    Guid.TryParse(dehMeteredEvent.ClientTenantId, out tenantId))
                        {
                            var billingTags = $"{{\"AccountId\":\"{meteredEvent.AccountId}\",";
                            billingTags += $"\"TenantId\":\"{dehMeteredEvent.ClientTenantId}\",";
                            billingTags += $"\"ConsumedUnit\":\"Data Management Processing Unit\",";
                            billingTags += $"\"SKU\":\"{this.getProcessSKU(dehMeteredEvent.ProcessingTier)}\",";
                            billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | billingTags: {billingTags}");

                            billingEvent = BillingEventHelper.CreateProcessingUnitBillingEvent(new ProcessingUnitBillingEventParameters
                            {
                                //Guid.Parse(dehMeteredEvent.MDQBatchId), // EventId is use for dedup downstream - handle with care
                                EventId = Guid.Parse(dehMeteredEvent.JobId),
                                TenantId = Guid.Parse(dehMeteredEvent.ClientTenantId),  //Guid.Parse(dehMeteredEvent.TenantId),
                                CreationTime = now,
                                Quantity = dehMeteredEvent.ProcessingUnits,
                                BillingTags = billingTags,
                                BillingStartDate = dehMeteredEvent.JobStartTime.DateTime,
                                BillingEndDate = dehMeteredEvent.JobEndTime.DateTime,
                                SKU = this.getProcessSKU(dehMeteredEvent.ProcessingTier),
                                LogOnly = false
                            });
                        }
                        else
                        {
                            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | skipping logging:{JsonConvert.SerializeObject(meteredEvent)}");
                        }

                    }
                    else if (meteredEvent is GovernedAssetsMeteredEvent governedAssetsMeteredEvent)
                    {
                        var billingTags = $"{{\"AccountId\":\"{meteredEvent.AccountId}\",";
                        billingTags += $"\"TenantId\":\"{meteredEvent.TenantId}\",";
                        billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                        billingEvent = BillingEventHelper.CreateGovernedAssetCountBillingEvent(new GovernedAssetCountBillingEventParameters
                        {
                            EventId = Guid.Parse(governedAssetsMeteredEvent.JobId), // EventId is use for dedup downstream - handle with care
                            TenantId = Guid.Parse(governedAssetsMeteredEvent.TenantId),
                            CreationTime = now,
                            Quantity = governedAssetsMeteredEvent.CountOfGovernedAssets,
                            BillingTags = billingTags,
                            BillingStartDate = governedAssetsMeteredEvent.ProcessingTimestamp.DateTime,
                            BillingEndDate = governedAssetsMeteredEvent.ProcessingTimestamp.DateTime,
                            LogOnly = false
                        });
                    }
                    else
                    {
                        throw new NotImplementedException($"Unsupported meteredEvent type: {meteredEvent.GetType().Name}");
                    }

                    return billingEvent;

                }).ToList();

                if (batch.Count > 0)
                {
                    var finalBatch = batch.Where(meteredEvent => meteredEvent != null).ToList();
                    if (finalBatch.Count > 0)
                    {
                        await this.EmitEventsWithRetryAndErrorHandling($"{this.GetType().Name}:|Batch number {currentBatch}. Batch Size {batchSize}. Emitting batch size: {finalBatch.Count}, Emitted so far.... {(currentBatch * batchSize) + batchSize} billable events",
                                            finalBatch, Guid.NewGuid());
                    }
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

    private BillingSKU getProcessSKU(string ProcessingTier)
    {
        switch (ProcessingTier.ToLowerInvariant())
        {
            case "basic":
                return BillingSKU.Basic;

            case "Standard":
                return BillingSKU.Standard;

            case "Advanced":
                return BillingSKU.Advanced;

            default:
                return BillingSKU.Basic;
        }

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

    private async Task EmitEventsWithRetryAndErrorHandling(string logShared, List<ExtendedBillingEvent> billingEvents, Guid correlationId)
    {

        //this.logger.LogInformation($"{this.GetType().Name}:|Before Removing null values {JsonConvert.SerializeObject(billingEventsList)}");
        //var billingEvents = billingEventsList.Where(q => q != null).ToList();
        //this.logger.LogInformation($"{this.GetType().Name}:|After Removing null values{JsonConvert.SerializeObject(billingEvents)}");

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
                    BillingTimestamp = q.BillingEvent.CreationTime,
                    BillingEvent = JsonConvert.SerializeObject(q),
                    CorrelationId = q.BillingEvent.CorrelationId.ToString(),
                    EventId = q.BillingEvent.Id.ToString(),
                    BatchId = correlationId.ToString(),
                    Service = q.BillingEvent.SolutionName,
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
