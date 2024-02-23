namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StorageSasRequest = Azure.Purview.DataEstateHealth.Models.StorageSasRequest;

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

    public Task<IEnumerable<ScorePayload>> ParseDQResult(string accountId, string dataProductId, string dataAssetId, string jobId)
    {
        throw new NotImplementedException();
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
        var storageSasRequest = new StorageSasRequest()
        {
            Path = "/",
            Permissions = "rl", // Only read permissions
            TimeToLive = TimeSpan.FromHours(7)
        };

        var uri = await this.processingStorageManager.GetProcessingStorageSasUri(
            accountStorageModel,
            storageSasRequest,
            accountStorageModel.CatalogId.ToString(),
            CancellationToken.None).ConfigureAwait(false);
        var uriStr = uri.ToString();
        var delimiterIndex = uriStr.IndexOf("?");
        var sasToken = uriStr.Substring(delimiterIndex + 1);

        var jobId = await dataQualityServiceClient.TriggerJobRun(
            accountId,
            dataProductId,
            dataAssetId,
            new JobSubmitPayload(
                sasToken,
                dfsEndpoint,
                catalogId,
                dataProductId,
                dataAssetId,
                healthJobId)).ConfigureAwait(false);

        return jobId;
    }
}
