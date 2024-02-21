namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

    public Task<IEnumerable<ScorePayload>> ParseDQResult(string accountId, string dataProductId, string dataAssetId, string jobId)
    {
        throw new NotImplementedException();
    }

    // TODO Will add more params later
    public async Task<string> SubmitDQJob(string accountId)
    {
        // Query storage account
        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);
        var storageAccountName = accountStorageModel.GetStorageAccountName();
        var dfsEndpoint = accountStorageModel.GetDfsEndpoint();

        // Convert to an observer
        var observerAdapter = new ObserverAdapter(
            dfsEndpoint,
            accountId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());
        var observer = observerAdapter.FromControlAssessment();

        observer.ExecutionData = new JObject()
        {
            { DataEstateHealthConstants.DEH_KEY_DATA_SOURCE_ENDPOINT, dfsEndpoint }
        };

        var str = JsonConvert.SerializeObject(observer.JObject);

        // Create a temporary observer
        var dataQualityServiceClient = this.dataQualityServiceClientFactory.GetClient();
        await dataQualityServiceClient.CreateObserver(observer).ConfigureAwait(false);

        // Trigger run
        var jobId = await dataQualityServiceClient.TriggerJobRun(
            observer.DataProduct.ReferenceId,
            observer.DataAsset.ReferenceId).ConfigureAwait(false);

        return jobId;
    }
}
