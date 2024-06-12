// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.DeltaWriter;
using Newtonsoft.Json;

internal class DataQualityEventsProcessor : PartnerEventsProcessor
{
    private readonly DataQualityEventHubConfiguration eventHubConfiguration;

    private readonly IDataHealthApiService dataHealthApiService;

    public DataQualityEventsProcessor(
        IServiceProvider serviceProvider,
        DataQualityEventHubConfiguration eventHubConfiguration,
        IDataHealthApiService dataHealthApiService)
        : base(
            serviceProvider,
            eventHubConfiguration,
            EventSourceType.DataQuality)
    {
        this.eventHubConfiguration = eventHubConfiguration;
        this.dataHealthApiService = dataHealthApiService;
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

        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> sourceModels = this.ParseSourcePayloads(events);
        this.DataEstateHealthRequestLogger.LogTrace($"Parsed {sourceModels.Values.Sum(i => i.Count)} {this.EventProcessorType} source events for account Id: {accountId}.");

        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> mdqSourceModels = this.FitlerDataQualitySourceEvents(sourceModels, true);

        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> mdqJobModels = this.UpdateMDQJobStatus(mdqSourceModels);
        this.DataEstateHealthRequestLogger.LogTrace($"Updated {mdqJobModels.Values.Sum(i => i.Count)} {this.EventProcessorType} MDQ events for account Id: {accountId}.");
    }

    private Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> FitlerDataQualitySourceEvents(Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> sourceModels, bool pickMDQ)
    {
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> result = new();
        foreach (var sourceModel in sourceModels)
        {
            result[sourceModel.Key] = sourceModel.Value.Where((item) => pickMDQ ? this.CheckMDQSourceEvent(item) : !this.CheckMDQSourceEvent(item)).ToList();
        }
        return result;
    }

    private bool CheckMDQSourceEvent(DataQualitySourceEventHubEntityModel sourceModel)
    {
        var domainModelPayload = this.ParseDomainModelPayload(sourceModel.Domainmodel);
        return domainModelPayload != null && domainModelPayload.JobType == "MDQ";
    }

    private Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> ParseSourcePayloads(List<EventHubModel> events)
    {
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModels = new();

        foreach (EventHubModel eventHubModel in events)
        {
            switch (eventHubModel.PayloadKind)
            {
                case PayloadKind.DataQualityFact:
                    this.DataEstateHealthRequestLogger.LogTrace($"Start to parse data quality event: {eventHubModel.AlternatePayload}");
                    this.ParseEventPayload(eventHubModel, dataQualityResultModels);
                    break;
                default:
                    this.DataEstateHealthRequestLogger.LogWarning($"Encountered unsupported {this.EventProcessorType} event kind: {JsonConvert.SerializeObject(eventHubModel)}.");
                    break;
            }
        }
        return dataQualityResultModels;
    }

    private Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> UpdateMDQJobStatus(
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModels)
    {
        Dictionary<EventOperationType, List<DataQualitySourceEventHubEntityModel>> mdqJobModels = new();
        foreach (KeyValuePair<EventOperationType, List<DataQualitySourceEventHubEntityModel>> dataQualityResultModel in dataQualityResultModels)
        {
            List<DataQualitySourceEventHubEntityModel> sourceModels = dataQualityResultModel.Value;
            var jobModels = new List<DataQualitySourceEventHubEntityModel>();
            mdqJobModels.Add(dataQualityResultModel.Key, jobModels);
            foreach (DataQualitySourceEventHubEntityModel sourceModel in sourceModels)
            {
                var domainModelPayload = this.ParseDomainModelPayload(sourceModel.Domainmodel);
                var resultId = this.ParseResultId(sourceModel.ResultId);
                if (!Guid.TryParse(resultId.JobId, out var dqJobId) || !Guid.TryParse(domainModelPayload.TenantId, out var tenantId) || !Guid.TryParse(sourceModel.AccountId, out var accountId))
                {
                    this.DataEstateHealthRequestLogger.LogInformation($"Ignore MDQ job with invalid id. JobId: {resultId.JobId}. TenantId: {domainModelPayload.TenantId}. AccountId: {sourceModel.AccountId}.");
                    continue;
                }
                this.DataEstateHealthRequestLogger.LogInformation($"Process MDQ job. Job ID: {dqJobId}. Tenant ID: {tenantId}. Account ID: {accountId}. Job status: {sourceModel.JobStatus}.");
                var model = new MDQJobModel
                {
                    DQJobId = dqJobId,
                    JobStatus = sourceModel.JobStatus,
                    TenantId = tenantId,
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                };
                this.dataHealthApiService.TriggerMDQJobCallback(model, false, CancellationToken.None);
                jobModels.Add(sourceModel);
            }
        }
        return mdqJobModels;
    }

    private ResultIdEventHubEntityModel ParseResultId(string resultId)
    {
        try
        {
            ResultIdEventHubEntityModel jobRunId = JsonConvert.DeserializeObject<ResultIdEventHubEntityModel>(resultId ?? string.Empty);
            return jobRunId;
        }
        catch (JsonException exception)
        {
            this.DataEstateHealthRequestLogger.LogError($"Failed to parse data quality job run id payload: {resultId}.", exception);
            throw;
        }
    }

    private DomainModelEventHubPayloadEntityModel ParseDomainModelPayload(string domainmodel)
    {
        try
        {
            DomainModelEventHubEntityModel domainModel = JsonConvert.DeserializeObject<DomainModelEventHubEntityModel>(domainmodel ?? string.Empty);
            return domainModel.Payload;
        }
        catch (JsonException exception)
        {
            this.DataEstateHealthRequestLogger.LogError($"Failed to parse domain model payload: {domainmodel}.", exception);
            return null;
        }
    }
}
