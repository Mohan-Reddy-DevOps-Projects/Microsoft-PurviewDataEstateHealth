namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
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
using System.Threading;
using System.Threading.Tasks;

public class DataQualityExecutionService : IDataQualityExecutionService
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly DataQualityServiceClientFactory dataQualityServiceClientFactory;

    public DataQualityExecutionService(
        IProcessingStorageManager processingStorageManager,
        DataQualityServiceClientFactory dataQualityServiceClientFactory)
    {
        this.processingStorageManager = processingStorageManager;
        this.dataQualityServiceClientFactory = dataQualityServiceClientFactory;
    }

    public async Task<IEnumerable<DHRawScore>> ParseDQResult(
        string accountId,
        string controlId,
        string healthJobId,
        string dqJobId)
    {
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

        return result;
    }

    // TODO Will add more params later
    public async Task<string> SubmitDQJob(string accountId, string controlId, string healthJobId)
    {
        // Query storage account
        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);
        var dfsEndpoint = accountStorageModel.GetDfsEndpoint();
        var catalogId = accountStorageModel.CatalogId.ToString();

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
        var str = JsonConvert.SerializeObject(observer.JObject);
        await dataQualityServiceClient.Test().ConfigureAwait(false);

        // Create a temporary observer
        await dataQualityServiceClient.CreateObserver(observer, accountId).ConfigureAwait(false);

        // Trigger run
        var jobId = await dataQualityServiceClient.TriggerJobRun(
            accountId,
            dataProductId,
            dataAssetId,
            new JobSubmitPayload(
                dfsEndpoint,
                catalogId,
                dataProductId,
                dataAssetId)).ConfigureAwait(false);

        return jobId;
    }
}
