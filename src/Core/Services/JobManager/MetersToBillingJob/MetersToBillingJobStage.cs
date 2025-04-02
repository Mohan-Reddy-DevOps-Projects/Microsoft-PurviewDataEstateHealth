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
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
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
using DEH.Application.Abstractions.Catalog;
using Microsoft.Extensions.Logging;

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
    private readonly ICatalogHttpClientFactory catalogHttpClientFactory;

    private string workspaceId;
    private string workspaceKey;
    private readonly IAccountExposureControlConfigProvider exposureControl;

    public enum QueryType
    {
        Deh,
        Dq,
        BYOC
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
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
        this.catalogHttpClientFactory = scope.ServiceProvider.GetService<ICatalogHttpClientFactory>();

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
            finalExecutionStatusDetails += await this.ProcessBilling<DEHMeteredEvent>("PDG_byoc_billing.kql", QueryType.BYOC, pollFrom, utcNow);

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

    private async Task LogProcessedJobs(string kql, DateTimeOffset fromDate, DateTimeOffset toDate, List<CBSBillingReceipt> receipts)
    {
        var started = DateTimeOffset.UtcNow;
        const string table = "DEHProcessedJobs_CL";

        try
        {
            // Query DEHProcessedJobs based on KQL
            var dehJobs = await this.logsAnalyticsReader.Query<DEHProcessedJobs>(await this.LoadKQL(kql), fromDate, toDate);

            // Proceed if there are DEH jobs and receipts
            if (dehJobs?.Value?.Any() == true && receipts.Any())
            {
                // Extract succeeded receipt EventIds
                var receiptEventIds = receipts
                    .Where(r => r.Status == CBSReceiptStatus.Succeded)
                    .Select(r => r.EventId)
                    .ToHashSet();
                this.logger.LogInformation($"Successful receipt count: {receiptEventIds.Count}");

                // Filter DEHProcessedJobs based on receipt EventIds
                var filteredDehJobs = dehJobs.Value.Where(dehJob => receiptEventIds.Contains(dehJob.MDQBatchId)).ToList();
                this.logger.LogInformation($"Filtered DEH Job Count: {filteredDehJobs.Count}");

                if (filteredDehJobs.Any())
                {
                    // Send filtered jobs to the writer
                    await this.logsAnalyticsWriter.SendLogEntries<DEHProcessedJobs>(filteredDehJobs, table);

                    // Log each processed job
                    foreach (var dehJob in filteredDehJobs)
                    {
                        this.logger.LogInformation($"Emitting Processed Jobs: {dehJob.ToJson()}");
                    }
                }
                else
                {
                    this.logger.LogInformation("No matching DEH jobs found for successful receipts.");
                }
            }
            else
            {
                this.logger.LogInformation("No DEH jobs or receipts available for processing.");
            }
        }
        catch (Exception ex)
        {
            var duration = (DateTimeOffset.UtcNow - started).TotalSeconds;
            var errorMessage = ex != null ? $" Reason: {ex.Message}" : string.Empty;
            var status = $"KQL: {kql} | Status: Failed | Duration: {duration} seconds | {errorMessage} | DateTime: {DateTimeOffset.UtcNow}";
            this.logger.LogError(status);
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
                var receipts = await this.ProcessBillingEventsWithSplit<T>(kql, fromDate, toDate);
                var totalEvents = receipts.Count();
                if (queryType == QueryType.Deh)
                {
                    await this.LogProcessedJobs("PDG_deh_jobs.kql", fromDate, toDate, receipts);
                }
                else if (queryType == QueryType.BYOC)
                {
                    await this.LogProcessedJobs("PDG_byoc_jobs.kql", fromDate, toDate, receipts);
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

    private async Task<List<CBSBillingReceipt>> ProcessBillingEvents(List<DEHMeteredEvent> meteredEvents, DateTimeOffset toDate)
    {
        // Query for Meters:
        // pwdg_billing_deh (or any other function involved in billing) must join CBSBillingReceipts_CL by using EvendId_g and
        // only emit those billable events not having a CBSBillingReceipts_CL.Status_s  == "Succeded" 
        // 
        // This approach ensures auto-healing/auto-retry of all billable events for the last 7 days.        
        List<CBSBillingReceipt> receipts = new List<CBSBillingReceipt>();
        var billingReceipts = "CBSBillingReceipts_CL";
        try
        {
            var meteredEvent = await this.logsAnalyticsReader.Query<CBSBillingReceipt>(billingReceipts, DateTimeOffset.UtcNow.AddDays(-1), toDate);
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

        int totalEvents = 0;

        // if we get any data
        if (meteredEvents != null && meteredEvents.Count > 0)
        {
            this.logger.LogInformation($"{this.GetType().Name}:|{meteredEvents.Count.ToString()}");
            // update from last time poll
            this.metadata.LastPollTime = toDate.ToString();

            totalEvents = meteredEvents.Count;

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
                var batch = meteredEvents.Skip(currentBatch * batchSize).Take(batchSize).ToList()
                    .Where(meteredEvent => meteredEvent != null)
                    .Where(meteredEvent =>
                    {
                        if (meteredEvent is DEHMeteredEvent dehMeteredEvent)
                        {
                            if (toDate.Subtract(dehMeteredEvent.JobEndTime).TotalHours > 24)
                            {
                                return false;  // Skip this item
                            }
                        }
                        return true;  // Keep this item
                    })
                    .Select(meteredEvent =>
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
                                var billingTags = $"{{\"ConsumedUnit\":\"Data Management Processing Unit\",";
                                billingTags += $"\"SKU\":\"{this.getProcessSKU(dehMeteredEvent.ProcessingTier)}\",";
                                billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | billingTags: {billingTags}");

                                var ecDEHBillingEnabled = this.exposureControl.IsDEHBillingEventEnabled(dehMeteredEvent.AccountId, string.Empty, dehMeteredEvent.TenantId);

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
                                    LogOnly = !ecDEHBillingEnabled
                                });
                            }
                            else if (meteredEvent.DMSScope.ToUpperInvariant() == "BYOC")
                            {
                                var billingTags = $"{{\"ConsumedUnit\":\"Data Management Processing Unit\",";
                                billingTags += $"\"SKU\":\"{this.getProcessSKU(dehMeteredEvent.ProcessingTier)}\",";
                                billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | billingTags: {billingTags}");

                                var ecDEHBillingEnabled = this.exposureControl.IsDEHBillingEventEnabled(dehMeteredEvent.AccountId, string.Empty, dehMeteredEvent.TenantId);

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
                                    LogOnly = !ecDEHBillingEnabled
                                });
                            }
                            else if (meteredEvent.DMSScope.ToUpperInvariant() == "DQ" && Guid.TryParse(dehMeteredEvent.JobId, out jobIdGuid) &&
                                        Guid.TryParse(dehMeteredEvent.ClientTenantId, out tenantId))
                            {
                                var billingTags = $"{{\"ConsumedUnit\":\"Data Management Processing Unit\",";
                                billingTags += $"\"SKU\":\"{this.getProcessSKU(dehMeteredEvent.ProcessingTier)}\",";
                                billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

                                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | billingTags: {billingTags}");

                                var ecDQBillingEnabled = this.exposureControl.IsDQBillingEventEnabled(dehMeteredEvent.AccountId, string.Empty, dehMeteredEvent.TenantId);

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
                                    LogOnly = !ecDQBillingEnabled
                                });
                            }
                            else
                            {
                                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | skipping logging:{JsonConvert.SerializeObject(meteredEvent)}");
                            }

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
                        receipts.AddRange(await this.EmitEventsWithRetryAndErrorHandling($"{this.GetType().Name}:|Batch number {currentBatch}. Batch Size {batchSize}. Emitting batch size: {finalBatch.Count}, Emitted so far.... {(currentBatch * batchSize) + batchSize} billable events",
                                            finalBatch, Guid.NewGuid()));
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
        return receipts;
    }

    /// <summary>
    /// Processes billing events obtained from Log Analytics or test data for metering.
    /// </summary>
    /// <typeparam name="T">Type of metered event, must inherit from MeteredEvent</typeparam>
    /// <param name="DEHBillingProcesingKQL">KQL query name to use for retrieving metered events</param>
    /// <param name="fromDate">Start date for billing period</param>
    /// <param name="toDate">End date for billing period</param>
    /// <returns>A list of billing receipts</returns>
    private async Task<List<CBSBillingReceipt>> ProcessBillingEventsWithSplit<T>(string DEHBillingProcesingKQL, DateTimeOffset fromDate, DateTimeOffset toDate) where T : MeteredEvent
    {
        const string billingReceipts = "CBSBillingReceipts_CL";
        List<CBSBillingReceipt> receipts = new List<CBSBillingReceipt>();

        try
        {
            // Step 1: Ensure billing receipts table exists
            await this.EnsureBillingReceiptsTableExists(billingReceipts, fromDate, toDate);
            
            // Original code for querying Log Analytics:
            // Step 2: Query Log Analytics for metered events and convert to DEHMeteredEvent list
            var meteredEventsResponse = await this.logsAnalyticsReader.Query<T>(await this.LoadKQL(DEHBillingProcesingKQL), fromDate, toDate);
            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Querying {DEHBillingProcesingKQL} from {fromDate} to {toDate}");
            List<DEHMeteredEvent> meteredEvents = this.SerializeMeteredEventsToList(meteredEventsResponse);
            /*
            // Step 2: Generate dummy metered events instead of querying Log Analytics
            List<DEHMeteredEvent> meteredEvents = this.GenerateDummyMeteredEvents(DEHBillingProcesingKQL);
            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Generated dummy data instead of querying {DEHBillingProcesingKQL} from {fromDate} to {toDate}");
            */
            // Step 3: Partition events based on billing split configuration
            (List<DEHMeteredEvent> billingSplitEnabledEvents, List<DEHMeteredEvent> billingSplitDisabledEvents) = this.PartitionEventsByBillingSplit(meteredEvents);

            // Step 4: Process events with domain-based billing (enabled for split)
            if (billingSplitEnabledEvents.Any())
            {
                var billingSplitEnabledReceipts = await this.ProcessEventsWithDomain(billingSplitEnabledEvents, toDate);
                receipts.AddRange(billingSplitEnabledReceipts);
            }

            // Step 5: Process events without domain-based billing (disabled for split)
            if (billingSplitDisabledEvents.Any())
            {
                var billingSplitDisabledReceipts = await this.ProcessBillingEvents(billingSplitDisabledEvents, toDate);
                receipts.AddRange(billingSplitDisabledReceipts);
            }

            return receipts;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{this.GetType().Name}:|{this.StageName} | Error processing billing events: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Partitions events into two lists based on billing split configuration from exposure control.
    /// </summary>
    /// <param name="meteredEvents">The list of metered events to partition</param>
    /// <returns>A tuple containing enabled and disabled events</returns>
    private (List<DEHMeteredEvent> enabledEvents, List<DEHMeteredEvent> disabledEvents) PartitionEventsByBillingSplit(List<DEHMeteredEvent> meteredEvents)
    {
        List<DEHMeteredEvent> enabledBillingSplitEvents = new List<DEHMeteredEvent>();
        List<DEHMeteredEvent> disabledBillingSplitEvents = new List<DEHMeteredEvent>();

        foreach (var dehEvent in meteredEvents)
        {
            bool isBillingSplitEnabled = dehEvent.DMSScope == "DQ"
                ? this.exposureControl.IsDQBillingSplitEnabled(dehEvent.AccountId, string.Empty, dehEvent.TenantId)
                : this.exposureControl.IsDEHBillingSplitEnabled(dehEvent.AccountId, string.Empty, dehEvent.TenantId);
            
            if (isBillingSplitEnabled)
            {
                enabledBillingSplitEvents.Add(dehEvent);
                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Billing split enabled for event: {dehEvent.MDQBatchId}, AccountId: {dehEvent.AccountId}, TenantId: {dehEvent.TenantId}");
            }
            else
            {
                disabledBillingSplitEvents.Add(dehEvent);
                this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Billing split disabled for event: {dehEvent.MDQBatchId}, AccountId: {dehEvent.AccountId}, TenantId: {dehEvent.TenantId}");
            }
        }

        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Total events: {meteredEvents.Count}, Enabled: {enabledBillingSplitEvents.Count}, Disabled: {disabledBillingSplitEvents.Count}");

        return (enabledBillingSplitEvents, disabledBillingSplitEvents);
    }

    /// <summary>
    /// Processes a list of metered events with domain-based billing and returns the billing receipts.
    /// </summary>
    /// <param name="dehMeteredEvents">The list of metered events to process</param>
    /// <param name="toDate">The end date for processing</param>
    /// <returns>A list of billing receipts</returns>
    private async Task<List<CBSBillingReceipt>> ProcessEventsWithDomain(List<DEHMeteredEvent> dehMeteredEvents, DateTimeOffset toDate)
    {
        List<CBSBillingReceipt> receipts = new List<CBSBillingReceipt>();
        string logPrefix = $"{this.GetType().Name}:|{this.StageName}";

        // Skip processing if there are no events
        if (dehMeteredEvents == null || !dehMeteredEvents.Any())
        {
            this.logger.LogInformation($"{logPrefix} | No events to process for domain-based billing");
            return receipts;
        }

        try
        {
            // Step 1: Assign correlation IDs and update last poll time
            List<DEHMeteredEvent> eventsWithCorrelationIds = this.AssignCorrelationIds(dehMeteredEvents);
            this.metadata.LastPollTime = toDate.ToString();

            // Step 2: Enrich events with business domain information
            List<DEHMeteredEvent> enrichedEvents = new List<DEHMeteredEvent>();
            foreach (var dehEvent in eventsWithCorrelationIds)
            {
                try
                {
                    var domainEvents = await this.EnrichMeteredEvent(dehEvent);
                    enrichedEvents.AddRange(domainEvents);
                    this.logger.LogInformation($"{logPrefix} | Enriched event {dehEvent.MDQBatchId} with {domainEvents.Count} dimensions");
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"{logPrefix} | Error enriching event {dehEvent.MDQBatchId}: {ex.Message}", ex);
                }
            }

            // Step 3: Process original events with receipt logging
            var originalReceipts = await this.ProcessEventsBatch(eventsWithCorrelationIds, toDate, true);
            receipts.AddRange(originalReceipts);

            // Step 4: Process enriched events without receipt logging
            if (enrichedEvents.Any())
            {
                var enrichedReceipts = await this.ProcessEventsBatch(enrichedEvents, toDate, false);
                // receipts.AddRange(enrichedReceipts);
            }

            return receipts;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{logPrefix} | Error in domain-based processing: {ex.Message}", ex);
            return receipts;
        }
    }

    /// <summary>
    /// Assigns correlation IDs to each metered event.
    /// </summary>
    private List<DEHMeteredEvent> AssignCorrelationIds(List<DEHMeteredEvent> events)
    {
        foreach (var dehEvent in events)
        {
            if (string.IsNullOrEmpty(dehEvent.CorrelationId))
            {
                dehEvent.CorrelationId = Guid.NewGuid().ToString();
            }
        }

        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Assigned correlation IDs to {events.Count} events");
        return events;
    }

    /// <summary>
    /// Processes a batch of metered events and returns the billing receipts.
    /// </summary>
    /// <param name="events">The list of events to process</param>
    /// <param name="toDate">The end date for processing</param>
    /// <param name="logReceipts">Whether to log the receipts</param>
    /// <returns>A list of billing receipts</returns>
    private async Task<List<CBSBillingReceipt>> ProcessEventsBatch(List<DEHMeteredEvent> events, DateTimeOffset toDate, bool logReceipts)
    {
        List<CBSBillingReceipt> receipts = new List<CBSBillingReceipt>();
        string logPrefix = $"{this.GetType().Name}:|{this.StageName}";

        // Early return if no events to process
        if (events == null || !events.Any())
        {
            this.logger.LogInformation($"{logPrefix} | No events to process in batch");
            return receipts;
        }

        const int batchSize = 25;
        int totalProcessed = 0;

        try
        {
            // Process events in batches
            for (int batchIndex = 0; ; batchIndex++)
            {
                // Get current batch of events
                var currentBatch = events
                    .Skip(batchIndex * batchSize)
                    .Take(batchSize)
                    .ToList();

                // Exit loop if no more events to process
                if (!currentBatch.Any())
                {
                    break;
                }

                // Filter out events older than 24 hours
                var filteredBatch = currentBatch
                    .Where(e => e != null && toDate.Subtract(e.JobEndTime).TotalHours <= 24)
                    .ToList();

                if (!filteredBatch.Any())
                {
                    continue;
                }

                // Generate billing events for all events in the batch
                var billingEvents = filteredBatch
                    .SelectMany(this.CreateBillingEventForMeteredEvent)
                    .Where(e => e != null)
                    .ToList();

                totalProcessed += billingEvents.Count;
                this.logger.LogInformation($"{logPrefix} | Batch {batchIndex}: Generated {billingEvents.Count} billing events");

                if (billingEvents.Any())
                {
                    // Submit billing events to billing service
                    var batchReceipts = await this.EmitEventsWithRetryAndErrorHandling(
                        $"{logPrefix} | Batch {batchIndex}: Size={batchSize}, Emitting {billingEvents.Count}, Total processed={totalProcessed}",
                        billingEvents,
                        Guid.NewGuid(),
                        logReceipts);

                    receipts.AddRange(batchReceipts);
                }
            }

            this.logger.LogInformation($"{logPrefix} | Completed batch processing: {totalProcessed} total billing events, {receipts.Count} receipts");
            return receipts;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{logPrefix} | Error processing batch: {ex.Message}", ex);
            return receipts;
        }
    }

    /// <summary>
    /// Creates billing events from a metered event based on its DMS scope.
    /// Returns both domain and non-domain versions of the event.
    /// </summary>
    private List<ExtendedBillingEvent> CreateBillingEventForMeteredEvent(DEHMeteredEvent meteredEvent)
    {
        var result = new List<ExtendedBillingEvent>();

        if (meteredEvent == null)
        {
            this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Cannot create billing event for null metered event");
            return result;
        }

        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(meteredEvent.DMSScope))
            {
                this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Missing DMSScope for event: {meteredEvent.MDQBatchId}");
                return result;
            }

            // Define scope mappings
            Dictionary<string, string> scopeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "DEH", "DEH_Controls" },
                { "BYOC", "DEH_BYOC" },
                { "DQ", "DQ" }
            };

            // Get scope name from mapping or default to "Unknown"
            string dmsScope = meteredEvent.DMSScope.ToUpperInvariant();
            string scopeName = scopeMappings.TryGetValue(dmsScope, out var mappedName)
                ? mappedName
                : "Unknown";

            DateTime now = DateTime.UtcNow;
            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Processing event: {meteredEvent.MDQBatchId}, Scope: {dmsScope}");

            // Create billing tags (common for all event types)
            string billingTags = $"{{\"ConsumedUnit\":\"Data Management Processing Unit\",";
            billingTags += $"\"SKU\":\"{this.getProcessSKU(meteredEvent.ProcessingTier)}\",";
            billingTags += $"\"SubSolutionName\":\"{scopeName}\"}}";

            // Create the billing parameters based on event type
            ProcessingUnitBillingEventParameters parameters = null;
            GovernedDomainProcessingUnitBillingEventParameters parametersWithDomain = null;

            switch (dmsScope)
            {
                case "DEH":
                    if (string.IsNullOrEmpty(meteredEvent.ReportingDimensions))
                    {
                        parameters = this.CreateBillingParameters(
                            meteredEvent,
                            Guid.Parse(meteredEvent.MDQBatchId),
                            Guid.Parse(meteredEvent.TenantId),
                            billingTags,
                            now,
                            this.exposureControl.IsDEHBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                        );
                    }
                    else
                    {
                        parametersWithDomain = this.CreateBillingParametersWithDomain(
                            meteredEvent,
                            Guid.NewGuid(),
                            Guid.Parse(meteredEvent.TenantId),
                            billingTags,
                            now,
                            this.exposureControl.IsDEHBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                        );
                    }
                    break;

                case "BYOC":
                    if (string.IsNullOrEmpty(meteredEvent.ReportingDimensions))
                    {
                        parameters = this.CreateBillingParameters(
                            meteredEvent,
                            Guid.Parse(meteredEvent.MDQBatchId),
                            Guid.Parse(meteredEvent.TenantId),
                            billingTags,
                            now,
                            this.exposureControl.IsDEHBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                        );
                    }
                    else
                    {
                        parametersWithDomain = this.CreateBillingParametersWithDomain(
                            meteredEvent,
                            Guid.NewGuid(),
                            Guid.Parse(meteredEvent.TenantId),
                            billingTags,
                            now,
                            this.exposureControl.IsDEHBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                        );
                    }
                    break;

                case "DQ":
                    // DQ requires specific ID parsing
                    if (Guid.TryParse(meteredEvent.JobId, out Guid jobIdGuid) &&
                        Guid.TryParse(meteredEvent.ClientTenantId, out Guid tenantId))
                    {
                        if (string.IsNullOrEmpty(meteredEvent.ReportingDimensions))
                        {
                            parameters = this.CreateBillingParameters(
                                meteredEvent,
                                jobIdGuid,
                                tenantId,
                                billingTags,
                                now,
                                this.exposureControl.IsDQBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                            );
                        }
                        else
                        {
                            parametersWithDomain = this.CreateBillingParametersWithDomain(
                                meteredEvent,
                                Guid.NewGuid(),
                                tenantId,
                                billingTags,
                                now,
                                this.exposureControl.IsDQBillingEventEnabled(meteredEvent.AccountId, string.Empty, meteredEvent.TenantId)
                            );
                        }
                    }
                    else
                    {
                        this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Invalid GUIDs in DQ event: JobId={meteredEvent.JobId}, ClientTenantId={meteredEvent.ClientTenantId}");
                    }
                    break;

                default:
                    this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Unsupported scope: {dmsScope}");
                    break;
            }

            // Create and add both events if parameters were created successfully
            if (parameters != null)
            {
                result.Add(BillingEventHelper.CreateProcessingUnitBillingEvent(parameters));
            }
            if (parametersWithDomain != null)
            {
                result.Add(BillingEventHelper.CreateDomainBasedProcessingUnitBillingEvent(parametersWithDomain));
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{this.GetType().Name}:|{this.StageName} | Error creating billing event: {ex.Message}", ex);
        }

        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Generated {result.Count} events for: {meteredEvent.MDQBatchId}");
        return result;
    }

    /// <summary>
    /// Creates ProcessingUnitBillingEventParameters from a metered event
    /// </summary>
    private ProcessingUnitBillingEventParameters CreateBillingParameters(
        DEHMeteredEvent meteredEvent,
        Guid eventId,
        Guid tenantId,
        string billingTags,
        DateTime creationTime,
        bool billingEnabled)
    {
        return new ProcessingUnitBillingEventParameters
        {
            EventId = eventId,
            TenantId = tenantId,
            CorrelationId = meteredEvent.CorrelationId,
            CreationTime = creationTime,
            Quantity = meteredEvent.ProcessingUnits,
            BillingTags = billingTags,
            BillingStartDate = meteredEvent.JobStartTime.DateTime,
            BillingEndDate = meteredEvent.JobEndTime.DateTime,
            SKU = this.getProcessSKU(meteredEvent.ProcessingTier),
            LogOnly = !billingEnabled
        };
    }

    private GovernedDomainProcessingUnitBillingEventParameters CreateBillingParametersWithDomain(
        DEHMeteredEvent meteredEvent,
        Guid eventId,
        Guid tenantId,
        string billingTags,
        DateTime creationTime,
        bool billingEnabled)
    {
        return new GovernedDomainProcessingUnitBillingEventParameters
        {
            EventId = eventId,
            TenantId = tenantId,
            CorrelationId = meteredEvent.CorrelationId,
            CreationTime = creationTime,
            Quantity = meteredEvent.ProcessingUnits,
            BillingTags = billingTags,
            BillingStartDate = meteredEvent.JobStartTime.DateTime,
            BillingEndDate = meteredEvent.JobEndTime.DateTime,
            SKU = this.getProcessSKU(meteredEvent.ProcessingTier),
            ReportingDimensions = meteredEvent.ReportingDimensions,
            LogOnly = !billingEnabled
        };
    }

    /// <summary>
    /// Serializes the metered events response from Log Analytics into a list of DEHMeteredEvent objects
    /// </summary>
    /// <param name="response">The response from Log Analytics query</param>
    /// <returns>A list of DEHMeteredEvent objects</returns>
    private List<DEHMeteredEvent> SerializeMeteredEventsToList(dynamic response)
    {
        // Early return for null or empty response
        if (response?.Value == null)
        {
            this.logger.LogInformation($"{this.GetType().Name}:|SerializeMeteredEventsToList | No metered events found in the response");
            return new List<DEHMeteredEvent>();
        }

        // Initialize result list with capacity hint if possible
        int estimatedCapacity = 0;
        try { estimatedCapacity = response.Value.Count; } catch { /* ignore if we can't get count */ }
        List<DEHMeteredEvent> result = new List<DEHMeteredEvent>(Math.Max(estimatedCapacity, 10));

        int successCount = 0;
        int errorCount = 0;

        foreach (var item in response.Value)
        {
            if (item == null)
            {
                continue;
            }

            try
            {
                // If item is already a DEHMeteredEvent, add it directly
                if (item is DEHMeteredEvent dehEvent)
                {
                    result.Add(dehEvent);
                    successCount++;
                    continue;
                }

                // Otherwise, serialize and deserialize to convert
                string json = JsonConvert.SerializeObject(item);
                DEHMeteredEvent convertedEvent = JsonConvert.DeserializeObject<DEHMeteredEvent>(json);

                if (convertedEvent != null)
                {
                    result.Add(convertedEvent);
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                this.logger.LogError($"{this.GetType().Name}:|SerializeMeteredEventsToList | Error converting event: {ex.Message}", ex);
            }
        }

        this.logger.LogInformation($"{this.GetType().Name}:|SerializeMeteredEventsToList | Converted {successCount} events successfully, {errorCount} failures out of {result.Count + errorCount} total events");
        return result;
    }

    /// <summary>
    /// Ensures that the billing receipts table exists in Log Analytics.
    /// If the table doesn't exist, it creates it with a default record.
    /// </summary>
    /// <param name="billingReceiptsTable">The name of the billing receipts table</param>
    /// <param name="fromDate">The from date for the query</param>
    /// <param name="toDate">The to date for the query</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task EnsureBillingReceiptsTableExists(string billingReceiptsTable, DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        try
        {
            var meteredEvent = await this.logsAnalyticsReader.Query<CBSBillingReceipt>(billingReceiptsTable, fromDate, toDate);
            this.logger.LogInformation($"{this.GetType().Name}:|{billingReceiptsTable} table found in Log Analytics.");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{this.GetType().Name}:|Error in Querying {billingReceiptsTable}, Table not found", ex);

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
            await this.logsAnalyticsWriter.SendLogEntries<CBSBillingReceipt>(coldStartReceipts, billingReceiptsTable);
            this.logger.LogInformation($"{this.GetType().Name}:|Created {billingReceiptsTable} table.");
        }
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

    /// <summary>
    /// Enriches meteredEvent with reporting dimensions for all business domains.
    /// Returns a list of meteredEvents, one for each business domain.
    /// </summary>
    private async Task<List<DEHMeteredEvent>> EnrichMeteredEvent(DEHMeteredEvent meteredEvent)
    {
        var result = new List<DEHMeteredEvent>();

        try
        {
            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Enriching metered event with reporting dimensions");

            // Skip enrichment if AccountId is missing or invalid
            if (string.IsNullOrEmpty(meteredEvent.AccountId))
            {
                this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Cannot enrich event: AccountId is missing");
                result.Add(meteredEvent); // Add the original event
                return result;
            }

            // Get the business domain details from the API
            if (this.catalogHttpClientFactory != null)
            {
                var catalogClient = this.catalogHttpClientFactory.GetClient();
                if (catalogClient != null)
                {
                    try
                    {
                        List<Microsoft.Purview.DataGovernance.Catalog.Model.Domain> domains = new List<Microsoft.Purview.DataGovernance.Catalog.Model.Domain>();

                        if (meteredEvent.DMSScope == "DQ" && !string.IsNullOrEmpty(meteredEvent.BusinessDomainId))
                        {
                            var domain = await catalogClient.GetBusinessDomainById(meteredEvent.BusinessDomainId, meteredEvent.AccountId, meteredEvent.TenantId);
                            if (domain != null)
                            {
                                domains.Add(domain);
                            }
                        }
                        else
                        {
                            domains = await catalogClient.GetAllBusinessDomains(meteredEvent.AccountId, meteredEvent.TenantId) ?? new List<Microsoft.Purview.DataGovernance.Catalog.Model.Domain>();
                        }

                        if (domains == null || !domains.Any())
                        {
                            this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | No business domains found for account: {meteredEvent.AccountId}");
                            result.Add(meteredEvent); // Add the original event
                            return result;
                        }

                        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Found {domains.Count()} business domains for account: {meteredEvent.AccountId}");

                        // Calculate per-domain processing units (divide evenly with 3 decimal places)
                        double processingUnitsPerDomain = Math.Round(meteredEvent.ProcessingUnits / domains.Count(), 3);

                        // Create a new event for each business domain
                        foreach (var domain in domains)
                        {
                            // Create a deep copy of the original event
                            var newEvent = new DEHMeteredEvent
                            {
                                // Copy all properties from the original event
                                TenantId = meteredEvent.TenantId,
                                AccountId = meteredEvent.AccountId,
                                DMSScope = meteredEvent.DMSScope,
                                ProcessingTimestamp = meteredEvent.ProcessingTimestamp,
                                ApplicationId = meteredEvent.ApplicationId,
                                ProcessingTier = meteredEvent.ProcessingTier,
                                CorrelationId = meteredEvent.CorrelationId,
                                EventCorrelationId = meteredEvent.EventCorrelationId,
                                MDQBatchId = meteredEvent.MDQBatchId,
                                JobDuration = meteredEvent.JobDuration,
                                BusinessDomainId = domain.Id.ToString(), // Set to current domain ID
                                JobStatus = meteredEvent.JobStatus,
                                ProcessingUnits = processingUnitsPerDomain, // Set to per-domain value with 3 decimal places
                                JobStartTime = meteredEvent.JobStartTime,
                                JobEndTime = meteredEvent.JobEndTime,
                                DMSJobSubType = meteredEvent.DMSJobSubType,
                                JobId = meteredEvent.JobId,
                                ClientTenantId = meteredEvent.ClientTenantId
                            };

                            // Set reporting dimensions for this domain
                            newEvent.ReportingDimensions = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                Dimensions = new[]
                                {
                                    new { Name = "Domain", Value = domain.Name },
                                    new { Name = "DomainId", Value = domain.Id.ToString() },
                                    new { Name = "ParentId", Value = domain.ParentId == null ? string.Empty : domain.ParentId.ToString() }
                                }
                            });

                            result.Add(newEvent);
                            this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Created enriched event for domain: {domain.Name} ({domain.Id}) with {processingUnitsPerDomain} processing units");
                        }

                        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Created {result.Count} enriched events from original event with evenly distributed processing units");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"{this.GetType().Name}:|{this.StageName} | Error retrieving business domains: {ex.Message}", ex);
                    }
                }
                else
                {
                    this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Cannot enrich event: Catalog client is null");
                }
            }
            else
            {
                this.logger.LogWarning($"{this.GetType().Name}:|{this.StageName} | Cannot enrich event: Catalog client factory is null");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{this.GetType().Name}:|{this.StageName} | Error in EnrichMeteredEvent: {ex.Message}", ex);
        }

        return result;
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

    private async Task<List<CBSBillingReceipt>> EmitEventsWithRetryAndErrorHandling(string logShared, List<ExtendedBillingEvent> billingEvents, Guid correlationId, bool logReceipts = true)
    {

        //this.logger.LogInformation($"{this.GetType().Name}:|Before Removing null values {JsonConvert.SerializeObject(billingEventsList)}");
        //var billingEvents = billingEventsList.Where(q => q != null).ToList();
        //this.logger.LogInformation($"{this.GetType().Name}:|After Removing null values{JsonConvert.SerializeObject(billingEvents)}");
        List<CBSBillingReceipt> receipts = new List<CBSBillingReceipt>();
        try
        {
            this.logger.LogInformation($"Emitting billingEvents -> {logShared}");

            foreach (var item in billingEvents)
            {
                this.logger.LogInformation($"Emitting billing event: {item.ToJson()}");
            }

            var response = await this.billingServiceClient.SubmitBillingEventsAsync(billingEvents, correlationId);

            // prepopulate all the CBS Receipts (optimistically succeded)
            receipts = billingEvents.Select(q =>
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

            // Save Receipts only if logReceipts is true
            if (logReceipts)
            {
                await this.logsAnalyticsWriter.SendLogEntries<CBSBillingReceipt>(receipts, "CBSBillingReceipts");
            }
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
        return receipts;
    }

    public bool IsStageComplete()
    {
        // we want this stage to run alwayas as it is was not completed
        return false;
    }

    /// <summary>
    /// Generates dummy DEH metered events for testing purposes.
    /// </summary>
    /// <param name="queryType">The type of query to generate dummy data for</param>
    /// <returns>A list of dummy DEH metered events</returns>
    private List<DEHMeteredEvent> GenerateDummyMeteredEvents(string queryType)
    {
        List<DEHMeteredEvent> dummyEvents = new List<DEHMeteredEvent>();

        // Generate different data based on query type
        string scope = "DEH";
        if (queryType.Contains("dq_billing"))
        {
            scope = "DQ";
        }
        else if (queryType.Contains("byoc_billing"))
        {
            scope = "BYOC";
        }

        // Generate 5 dummy events
        for (int i = 0; i < 2; i++)
        {
            var startTime = DateTimeOffset.UtcNow.AddHours(-i);
            var endTime = startTime.AddMinutes(15 + i * 5);
            var duration = (endTime - startTime).TotalSeconds;

            var dummyEvent = new DEHMeteredEvent
            {
                TenantId = "12d98746-0b5a-4778-8bd0-449994469062",
                AccountId = "ecf09339-34e0-464b-a8fb-661209048543",
                DMSScope = scope,
                ProcessingTimestamp = DateTimeOffset.UtcNow,
                ApplicationId = "app-" + Guid.NewGuid().ToString(),
                ProcessingTier = i % 3 == 0 ? "Basic" : (i % 3 == 1 ? "Standard" : "Advanced"),
                EventCorrelationId = Guid.NewGuid().ToString(),
                MDQBatchId = Guid.NewGuid().ToString(),
                JobDuration = duration,
                BusinessDomainId = scope == "DQ" ? "1b78fd34-8a4a-4c59-b1bb-7893fc168225" : null,
                JobStatus = "Completed",
                ProcessingUnits = 10 + i * 5,
                JobStartTime = startTime,
                JobEndTime = endTime,
                DMSJobSubType = "Process",
                JobId = Guid.NewGuid().ToString(),
                ClientTenantId = "12d98746-0b5a-4778-8bd0-449994469062",
                CorrelationId = Guid.NewGuid().ToString()
            };

            dummyEvents.Add(dummyEvent);
        }

        this.logger.LogInformation($"{this.GetType().Name}:|{this.StageName} | Generated {dummyEvents.Count} dummy events for {scope} scope");
        return dummyEvents;
    }
}