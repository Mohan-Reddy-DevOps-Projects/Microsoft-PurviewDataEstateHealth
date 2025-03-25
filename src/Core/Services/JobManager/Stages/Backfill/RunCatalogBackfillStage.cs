namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Stages.Backfill;

using DataAccess;
using DEH.Application.Abstractions.Catalog;
using DEH.Domain.Backfill.Catalog;
using Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Catalog.Model;
using ProjectBabylon.Metadata;
using System.Collections.Concurrent;
using WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class RunCatalogBackfillStage : IJobCallbackStage
{
    private readonly ConcurrentDictionary<string, int> _processedCounters = new();
    private readonly JobCallbackUtils<StartCatalogBackfillMetadata> _jobCallbackUtils;
    private readonly StartCatalogBackfillMetadata _metadata;
    private readonly IDataEstateHealthRequestLogger _logger;
    private readonly ICatalogHttpClientFactory _catalogHttpClientFactory;
    private readonly IMetadataAccessorService _metadataAccessorService;
    private readonly IBackfillCatalogRepository _okrRepository;
    private readonly IBackfillCatalogRepository _keyResultRepository;
    private readonly IBackfillCatalogRepository _cdeRepository;
    private readonly TimeSpan _defaultDelay = TimeSpan.FromMilliseconds(400);

    public RunCatalogBackfillStage(
        IServiceScope scope,
        StartCatalogBackfillMetadata metadata,
        JobCallbackUtils<StartCatalogBackfillMetadata> jobCallbackUtils,
        IMetadataAccessorService metadataAccessorService)
    {
        this._metadata = metadata;
        this._jobCallbackUtils = jobCallbackUtils;
        this._logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>() ?? throw new InvalidOperationException("Logger not found");
        this._catalogHttpClientFactory = scope.ServiceProvider.GetService<ICatalogHttpClientFactory>() ?? throw new InvalidOperationException("CatalogHttpClientFactory not found");
        this._metadataAccessorService = metadataAccessorService ?? throw new InvalidOperationException("MetadataAccessorService not found");
        this._okrRepository = scope.ServiceProvider.GetRequiredKeyedService<IBackfillCatalogRepository>("OkrBackfillCatalogRepository");
        this._keyResultRepository = scope.ServiceProvider.GetRequiredKeyedService<IBackfillCatalogRepository>("KeyresultBackfillCatalogRepository");
        this._cdeRepository = scope.ServiceProvider.GetRequiredKeyedService<IBackfillCatalogRepository>("CdeBackfillCatalogRepository");

        this._processedCounters["OKRs"] = 0;
        this._processedCounters["KeyResults"] = 0;
        this._processedCounters["CDEs"] = 0;
    }

    public string StageName => nameof(RunCatalogBackfillStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            this._logger.LogInformation($@"{this.StageName}|Starting global catalog backfill");
            this._metadata.BackfillStatus = CatalogBackfillStatus.InProgress;
            this._metadata.StartTime = DateTime.UtcNow;
            this._metadata.ItemsProcessed = 0;

            var accountIdsList = this._metadata.AccountIds;
            int batchAmount = this._metadata.BatchAmount;
            int bufferTimeInMinutes = this._metadata.BufferTimeInMinutes;

            this._logger.LogInformation($"Starting backfill for {accountIdsList.Count} accounts with batch size {batchAmount} and buffer time {bufferTimeInMinutes} minutes");

            for (int i = 0; i < accountIdsList.Count; i += batchAmount)
            {
                var currentBatch = accountIdsList.Skip(i)
                    .Take(batchAmount)
                    .ToList();
                this._logger.LogInformation($"Processing batch {i / batchAmount + 1} with {currentBatch.Count} accounts");

                await Parallel.ForEachAsync(currentBatch,
                    new ParallelOptions { MaxDegreeOfParallelism = 2 },
                    async (accountId, cancellationToken) =>
                    {
                        try
                        {
                            this._logger.LogInformation($"Processing account ID: {accountId}");
                            var metadataClient = this._metadataAccessorService.GetMetadataServiceClient();
                            var accountModel = await metadataClient.Accounts.GetAsync(accountId, "ByAccountId", cancellationToken: cancellationToken);
                            this._logger.LogInformation($"Retrieved account information for {accountModel.Name} (ID: {accountModel.Id}, Tenant: {accountModel.TenantId})");
                            var catalogClient = this._catalogHttpClientFactory.GetClient();
                            var businessDomains = await catalogClient.GetAllBusinessDomains(accountId, accountModel.TenantId);
                            this._logger.LogInformation($"Retrieved {businessDomains.Count} business domains for account {accountId}");

                            foreach (var domainId in businessDomains.Select(domain => domain.Id))
                            {
                                this._logger.LogInformation($"Processing business domain: {domainId} for account {accountId}");

                                // Process OKRs
                                var okrTask = Task.Run(async () =>
                                {
                                    var objectives = await catalogClient.GetAllOkrsForBusinessDomain(domainId, accountId, accountModel.TenantId);
                                    this._logger.LogInformation($"Retrieved {objectives.Count} OKRs for domain {domainId} in account {accountId}");

                                    if (objectives.Count > 0)
                                    {
                                        var processObjectivesTask = this.ProcessObjectivesAsync(objectives, accountId, accountModel.TenantId);

                                        // Process Key Results for each OKR
                                        var processKeyresultsTask = Task.Run(async () =>
                                        {
                                            foreach (var objective in objectives)
                                            {
                                                var keyResults = await catalogClient.GetAllKeyresultsForOkr(objective.Id, accountId, accountModel.TenantId);
                                                this._logger.LogInformation($"Retrieved {keyResults.Count} KeyResults for OKR {objective.Id} in account {accountId}");

                                                if (keyResults.Count > 0)
                                                {
                                                    await this.ProcessKeyResultsAsync(keyResults, objective.Id, accountId, accountModel.TenantId);
                                                }
                                            }
                                        }, cancellationToken);
                                        await Task.WhenAll(processObjectivesTask, processKeyresultsTask);
                                    }
                                }, cancellationToken);

                                // Process CDEs
                                var cdeTask = Task.Run(async () =>
                                {
                                    var criticalDataElements = await catalogClient.GetAllCdesForBusinessDomain(domainId, accountId, accountModel.TenantId);
                                    this._logger.LogInformation($"Retrieved {criticalDataElements.Count} CDEs for domain {domainId} in account {accountId}");

                                    if (criticalDataElements.Count > 0)
                                    {
                                        await this.ProcessCriticalDataElementsAsync(criticalDataElements, accountId, accountModel.TenantId);
                                    }
                                }, cancellationToken);

                                await Task.WhenAll(okrTask, cdeTask);
                                await this.WaitBetweenCallsToCatalog(cancellationToken);
                            }

                            await this._okrRepository.FlushAsync();
                            await this._keyResultRepository.FlushAsync();
                            await this._cdeRepository.FlushAsync();
                            this._logger.LogInformation($"Successfully processed account ID {accountId} with " +
                                                        $"{this._processedCounters["OKRs"]} OKRs, {this._processedCounters["KeyResults"]} KeyResults, and {this._processedCounters["CDEs"]} CDEs");
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError($"Error processing account ID {accountId}: {e.Message}", e);
                        }
                    });

                this._metadata.ItemsProcessed += currentBatch.Count;

                this._logger.LogInformation($"Current processing counts - OKRs: {this._processedCounters["OKRs"]}, KeyResults: {this._processedCounters["KeyResults"]}, CDEs: {this._processedCounters["CDEs"]}");

                // If there are more accounts to process, wait for the buffer time
                if (i + batchAmount >= accountIdsList.Count || bufferTimeInMinutes <= 0)
                {
                    continue;
                }

                this._logger.LogInformation($"Batch complete. Waiting for {bufferTimeInMinutes} minutes before processing the next batch");
                await Task.Delay(TimeSpan.FromMinutes(bufferTimeInMinutes))
                    .ConfigureAwait(false);
            }

            this._metadata.BackfillStatus = CatalogBackfillStatus.Completed;
            this._metadata.EndTime = DateTime.UtcNow;

            this._logger.LogInformation($"Catalog backfill completed. Final counts - OKRs: {this._processedCounters["OKRs"]}, KeyResults: {this._processedCounters["KeyResults"]}, CDEs: {this._processedCounters["CDEs"]}");

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"{this.StageName}|Global catalog backfill completed successfully. Processed {this._metadata.ItemsProcessed} accounts " +
                               $"with {this._processedCounters["OKRs"]} OKRs, {this._processedCounters["KeyResults"]} KeyResults, and {this._processedCounters["CDEs"]} CDEs.";
        }
        catch (Exception exception)
        {
            this._metadata.BackfillStatus = CatalogBackfillStatus.Failed;
            this._metadata.ErrorMessage = exception.Message;
            this._metadata.EndTime = DateTime.UtcNow;
            jobStageStatus = JobExecutionStatus.Failed;
            jobStatusMessage = $"{this.StageName}|Failed to process global catalog backfill with error: {exception.Message}";
            this._logger.LogError(jobStatusMessage, exception);
        }

        return this._jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    private async Task WaitBetweenCallsToCatalog(CancellationToken cancellationToken)
    {
        await Task.Delay(this._defaultDelay, cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessCriticalDataElementsAsync(List<CriticalDataElement> criticalDataElements, string accountId, string tenantId)
    {
        int processedCount = 0;
        foreach (var cde in criticalDataElements)
        {
            try
            {
                var dataChangeEvent = new DataChangeEvent<CriticalDataElement>
                {
                    EventId = Guid.NewGuid()
                        .ToString(),
                    CorrelationId = Guid.NewGuid()
                        .ToString(),
                    EventSource = EventSource.DataCatalog,
                    PayloadKind = PayloadKind.CriticalDataElement,
                    OperationType = OperationType.Create,
                    PreciseTimestamp = DateTime.UtcNow.ToString("o"),
                    TenantId = tenantId,
                    AccountId = accountId,
                    ChangedBy = "BackfillJob",
                    EventEnqueuedUtcTime = DateTime.UtcNow,
                    EventProcessedUtcTime = DateTime.UtcNow,
                    PartitionId = 0,
                    Payload = new DataChangeEventPayload<CriticalDataElement> { Before = null, After = cde, Related = null },
                    Id = Guid.NewGuid()
                        .ToString()
                };

                await this._cdeRepository.AddBatch(dataChangeEvent);
                processedCount++;

                this._logger.LogInformation($"Successfully processed CDE {cde.Id} for account {accountId}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error processing CDE {cde.Id} for account {accountId}: {ex.Message}", ex);
            }
        }

        this._processedCounters.AddOrUpdate("CDEs", processedCount, (key, oldValue) => oldValue + processedCount);
    }

    private async Task ProcessObjectivesAsync(List<ObjectiveWithAdditionalProperties> objectives, string accountId, string tenantId)
    {
        int processedCount = 0;
        foreach (var objective in objectives)
        {
            try
            {
                var dataChangeEvent = new DataChangeEvent<ObjectiveWithAdditionalProperties>
                {
                    EventId = Guid.NewGuid()
                        .ToString(),
                    CorrelationId = Guid.NewGuid()
                        .ToString(),
                    EventSource = EventSource.DataCatalog,
                    PayloadKind = PayloadKind.OKR,
                    OperationType = OperationType.Create,
                    PreciseTimestamp = DateTime.UtcNow.ToString("o"),
                    TenantId = tenantId,
                    AccountId = accountId,
                    ChangedBy = "BackfillJob",
                    EventEnqueuedUtcTime = DateTime.UtcNow,
                    EventProcessedUtcTime = DateTime.UtcNow,
                    PartitionId = 0,
                    Payload = new DataChangeEventPayload<ObjectiveWithAdditionalProperties> { Before = null, After = objective, Related = null },
                    Id = Guid.NewGuid()
                        .ToString()
                };

                await this._okrRepository.AddBatch(dataChangeEvent);
                processedCount++;

                this._logger.LogInformation($"Successfully processed OKR {objective.Id} for account {accountId}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error processing OKR {objective.Id} for account {accountId}: {ex.Message}", ex);
            }
        }

        this._processedCounters.AddOrUpdate("OKRs", processedCount, (key, oldValue) => oldValue + processedCount);
    }

    private async Task ProcessKeyResultsAsync(List<KeyResult> keyResults, Guid objectiveId, string accountId, string tenantId)
    {
        int processedCount = 0;
        foreach (var keyResult in keyResults)
        {
            try
            {
                var dataChangeEvent = new DataChangeEvent<KeyResult>
                {
                    EventId = Guid.NewGuid()
                        .ToString(),
                    CorrelationId = Guid.NewGuid()
                        .ToString(),
                    EventSource = EventSource.DataCatalog,
                    PayloadKind = PayloadKind.KeyResult,
                    OperationType = OperationType.Create,
                    PreciseTimestamp = DateTime.UtcNow.ToString("o"),
                    TenantId = tenantId,
                    AccountId = accountId,
                    ChangedBy = "BackfillJob",
                    EventEnqueuedUtcTime = DateTime.UtcNow,
                    EventProcessedUtcTime = DateTime.UtcNow,
                    PartitionId = 0,
                    Payload = new DataChangeEventPayload<KeyResult> { Before = null, After = keyResult, Related = null },
                    Id = Guid.NewGuid()
                        .ToString()
                };

                await this._keyResultRepository.AddBatch(dataChangeEvent);
                processedCount++;

                this._logger.LogInformation($"Successfully processed KeyResult {keyResult.Id} for OKR {objectiveId} and account {accountId}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error processing KeyResult {keyResult.Id} for OKR {objectiveId} and account {accountId}: {ex.Message}", ex);
            }
        }

        this._processedCounters.AddOrUpdate("KeyResults", processedCount, (key, oldValue) => oldValue + processedCount);
    }

    public bool IsStageComplete()
    {
        return this._metadata.BackfillStatus is CatalogBackfillStatus.Completed or CatalogBackfillStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}