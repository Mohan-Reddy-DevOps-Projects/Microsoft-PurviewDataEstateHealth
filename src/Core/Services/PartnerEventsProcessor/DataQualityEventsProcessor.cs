// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.DeltaWriter;
using Newtonsoft.Json;

internal class DataQualityEventsProcessor : PartnerEventsProcessor
{
    private readonly DataQualityEventHubConfiguration eventHubConfiguration;

    public DataQualityEventsProcessor(
        IServiceProvider serviceProvider,
        DataQualityEventHubConfiguration eventHubConfiguration)
        : base(
            serviceProvider,
            eventHubConfiguration,
            EventSourceType.DataQuality)
    {
        this.eventHubConfiguration = eventHubConfiguration;
    }

    public override async Task CommitAsync(IDictionary<Guid, string> processingStorageCache = null)
    {
        if (processingStorageCache != null)
        {
            this.ProcessingStorageCache = processingStorageCache;
        }

        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");

        Dictionary<Guid, List<EventHubModel>> eventsByAccount = this.GetEventsByAccount<EventHubModel>();

        foreach (var accountEvents in eventsByAccount)
        {
            try
            {
                await this.UploadEventsForAccount(accountEvents.Key, accountEvents.Value);
            }
            catch (Exception exception)
            {
                this.DataEstateHealthRequestLogger.LogCritical($"Failed to upload {accountEvents.Value.Count} events of {this.EventProcessorType} for account: {accountEvents.Key}.", exception);
                this.EventArgsToCheckpoint.Remove(accountEvents.Key);
            }
        }

        await this.CommitCheckpoints();
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");
    }

    private async Task UploadEventsForAccount(Guid accountId, List<EventHubModel> events)
    {
        if (await this.ProcessingStorageExists(accountId) != true)
        {
            return;
        }

        string storageEndpoint = this.ProcessingStorageCache[accountId];
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to persisting {this.EventProcessorType} events under {storageEndpoint} for account Id: {accountId}.");
        IDeltaLakeOperator deltaTableWriter = this.DeltaWriterFactory.Build(new Uri(storageEndpoint), this.AzureCredentialFactory.CreateDefaultAzureCredential(new Uri(this.eventHubConfiguration.Authority)));

        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModels = await this.PrepareAndUploadSourcePayloads(events, deltaTableWriter);
        this.DataEstateHealthRequestLogger.LogTrace($"Persisted {dataQualityResultModels.Values.Sum(i => i.Count)} {this.EventProcessorType} source events for account Id: {accountId}.");

        Dictionary<EventOperationType, List<DataQualitySinkEventHubEntityModel>> dataQualityScoreModels = await this.PrepareAndUploadSinkPayloads(dataQualityResultModels, deltaTableWriter);
        this.DataEstateHealthRequestLogger.LogTrace($"Persisted {dataQualityScoreModels.Values.Sum(i => i.Count)} {this.EventProcessorType} sink events for account Id: {accountId}.");
    }

    private async Task<Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>>> PrepareAndUploadSourcePayloads(List<EventHubModel> events, IDeltaLakeOperator deltaTableWriter)
    {
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModels = new();

        foreach (EventHubModel eventHubModel in events)
        {
            switch (eventHubModel.PayloadKind)
            {
                case PayloadKind.DataQualityFact:
                    this.ParseEventPayload(eventHubModel, dataQualityResultModels);
                    break;
                default:
                    this.DataEstateHealthRequestLogger.LogWarning($"Encountered unsupported {this.EventProcessorType} event kind: {JsonConvert.SerializeObject(eventHubModel)}.");
                    break;
            }
        }

        await this.PersistToStorage(dataQualityResultModels, deltaTableWriter, nameof(EventSourceType.DataQuality));
        return dataQualityResultModels;
    }

    private async Task<Dictionary<EventOperationType, List<DataQualitySinkEventHubEntityModel>>> PrepareAndUploadSinkPayloads(
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModels,
        IDeltaLakeOperator deltaTableWriter)
    {
        Dictionary<EventOperationType, List<DataQualitySinkEventHubEntityModel>> dataQualityScoreModels = new();
        foreach (KeyValuePair<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModel in dataQualityResultModels)
        {
            List<DataQualitySourceEventHubEntityModel> sourceModels = dataQualityResultModel.Value;
            var scoreModels = new List<DataQualitySinkEventHubEntityModel>();
            dataQualityScoreModels.Add(dataQualityResultModel.Key, scoreModels);

            foreach (DataQualitySourceEventHubEntityModel sourceModel in sourceModels)
            {
                try
                {
                    ResultIdEventHubEntityModel jobRunId = JsonConvert.DeserializeObject<ResultIdEventHubEntityModel>(sourceModel.ResultId ?? string.Empty);
                    if (jobRunId == null || jobRunId.BusinessDomainId == null || jobRunId.DataProductId == null || jobRunId.DataAssetId == null)
                    {
                        this.DataEstateHealthRequestLogger.LogWarning($"Encountered invalid data quality job run result: {JsonConvert.SerializeObject(sourceModel.ResultId)}");
                        continue;
                    }

                    var sinkModel = new DataQualitySinkEventHubEntityModel()
                    {
                        AccountId = sourceModel.AccountId,
                        BusinessDomainId = jobRunId.BusinessDomainId,
                        DataProductId = jobRunId.DataProductId,
                        DataAssetId = jobRunId.DataAssetId,
                        JobId = jobRunId.JobId,
                        RowId = sourceModel.EventId.ToString(),
                        ResultedAt = sourceModel.ResultedAt,
                        QualityScore = CalculateDataQualityScore(sourceModel),
                    };

                    scoreModels.Add(sinkModel);
                }
                catch (JsonException exception)
                {
                    this.DataEstateHealthRequestLogger.LogError($"Failed to parse data quality job run id payload from event: {JsonConvert.SerializeObject(sourceModel)}.", exception);
                }
            }
        }

        await this.PersistToStorage(dataQualityScoreModels, deltaTableWriter, nameof(EventSourceType.DataQuality), false);
        return dataQualityScoreModels;
    }

    private double CalculateDataQualityScore(DataQualitySourceEventHubEntityModel sourceModel)
    {
        double qualityScore = 0.0;

        if (sourceModel.JobStatus == nameof(JobRunState.Succeeded))
        {
            var ruleResults = JsonConvert.DeserializeObject<Dictionary<string, JobResultValuesEventHubEntityModel>>(sourceModel.Results);
            if (ruleResults != null)
            {
                int maxPassCount = 0;
                int totalPassCount = 0;
                foreach (JobResultValuesEventHubEntityModel ruleResult in ruleResults.Values)
                {
                    totalPassCount += ruleResult.PassedCount;
                    maxPassCount += ruleResult.PassedCount;
                    maxPassCount += ruleResult.FailedCount;
                    maxPassCount += ruleResult.MiscastCount;
                    maxPassCount += ruleResult.IgnoredCount;
                    maxPassCount += ruleResult.EmptyCount;
                    maxPassCount += ruleResult.UnevaluableCount;
                }

                qualityScore = 1.0 * totalPassCount / maxPassCount;
            }
        }

        return qualityScore;
    }
}
