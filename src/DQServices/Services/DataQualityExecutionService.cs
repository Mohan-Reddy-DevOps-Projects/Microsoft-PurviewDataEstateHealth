namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Clients;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DataQualityExecutionService : IDataQualityExecutionService
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly IDataQualityHttpClient dataQualityHttpClient;

    public DataQualityExecutionService(
        IProcessingStorageManager processingStorageManager,
        IDataQualityHttpClient dataQualityHttpClient)
    {
        this.processingStorageManager = processingStorageManager;
        this.dataQualityHttpClient = dataQualityHttpClient;
    }

    // TODO Will add more params later
    public async Task<string> SubmitDQJob(string accountId)
    {
        // Query storage account
        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);
        var storageAccountName = accountStorageModel.GetStorageAccountName();
        var dfsEndpoing = accountStorageModel.GetDfsEndpoint();

        // Convert to an observer
        var observer = ObserverAdapter.FromControlAssessment();

        // Create a temporary observer
        await this.dataQualityHttpClient.CreateObserver(observer).ConfigureAwait(false);

        // Trigger run
        var jobId = await this.dataQualityHttpClient.TriggerJobRun(
            observer.DataProduct.ReferenceId,
            observer.DataAsset.ReferenceId).ConfigureAwait(false);

        return jobId;
    }
}
