namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DataQualityExecutionService : IDataQualityExecutionService
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly DataQualityServiceClientFactory dataQualityServiceClientFactory;
    private readonly IDataEstateHealthRequestLogger logger;

    public DataQualityExecutionService(
        IProcessingStorageManager processingStorageManager,
        DataQualityServiceClientFactory dataQualityServiceClientFactory,
        IDataEstateHealthRequestLogger logger)
    {
        this.processingStorageManager = processingStorageManager;
        this.dataQualityServiceClientFactory = dataQualityServiceClientFactory;
        this.logger = logger;
    }

    public async Task<IEnumerable<DHRawScore>> ParseDQResult(
        string accountId,
        string controlId,
        string healthJobId,
        string dqJobId)
    {
        this.logger.LogInformation($"Start ParseDQResult, accountId:{accountId}, controlId:{controlId}, healthJobId:{healthJobId}, dqJobId:{dqJobId}");

        var dataProductId = controlId;
        var dataAssetId = healthJobId;

        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);

        var folderPath = ErrorOutputInfo.GeneratePartOfFolderPath(dataProductId, dataAssetId) + $"/observation={dqJobId}";

        var parquetStream = await this.processingStorageManager.GetDataQualityOutput(
            accountStorageModel,
            folderPath).ConfigureAwait(false);

        var memoryStream = new MemoryStream();
        await parquetStream.CopyToAsync(memoryStream).ConfigureAwait(false);

        var result = await DataQualityOutputAdapter.ToScorePayload(memoryStream).ConfigureAwait(false);

        this.logger.LogInformation($"End ParseDQResult, resultCount:{result.Count()}");
        if (result.Count() > 0)
        {
            this.logger.LogInformation($"End ParseDQResult, parsedRuleCount:{result.First().Scores.Count()}");
        }

        return result;
    }

    // TODO Will add more params later
    public async Task<string> SubmitDQJob(string accountId, string controlId, string healthJobId)
    {
        this.logger.LogInformation($"Start SubmitDQJOb, accountId:{accountId}, controlId:{controlId}, healthJobId:{healthJobId}");

        // Query storage account
        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);
        var dfsEndpoint = accountStorageModel.GetDfsEndpoint();
        var catalogId = accountStorageModel.CatalogId.ToString();

        this.logger.LogInformation($"Found storage account, dfsEndpoint:{dfsEndpoint}, catalogId:{catalogId}");

        var dataProductId = controlId;
        var dataAssetId = healthJobId;

        // Convert to an observer
        var observerAdapter = new ObserverAdapter(
            dfsEndpoint,
            catalogId,
            dataProductId,
            dataAssetId);
        var observer = observerAdapter.FromControlAssessment();

        observer.ExecutionData = new JObject()
        {
            { DataEstateHealthConstants.DEH_KEY_DATA_SOURCE_ENDPOINT, dfsEndpoint }
        };

        var dataQualityServiceClient = this.dataQualityServiceClientFactory.GetClient();

        // TODO For debug
        var observerPayload = JsonConvert.SerializeObject(observer.JObject);
        this.logger.LogInformation($"Observer payload: {observerPayload}");

        // Create a temporary observer
        await dataQualityServiceClient.CreateObserver(observer, accountId).ConfigureAwait(false);

        // Trigger run
        var dqJobId = await dataQualityServiceClient.TriggerJobRun(
            accountId,
            dataProductId,
            dataAssetId,
            new JobSubmitPayload(
                dfsEndpoint,
                catalogId,
                dataProductId,
                dataAssetId)).ConfigureAwait(false);

        this.logger.LogInformation($"End SubmitDQJOb, dqJobId:{dqJobId}");

        return dqJobId;
    }
}
