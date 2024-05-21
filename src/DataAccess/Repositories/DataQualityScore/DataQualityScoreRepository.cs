// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using System.Threading.Tasks;

internal class DataQualityScoreRepository : IDataQualityScoreRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private const int DefaultTimeout = 60 * 15;

    private IThreadLockService threadLockService;

    public DataQualityScoreRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         IThreadLockService threadLockService)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
        this.threadLockService = threadLockService;
    }

    public async Task<IBatchResults<DataQualityScoreEntity>> GetMultiple(
          DataQualityScoreKey dataQualityScoreKey,
          CancellationToken cancellationToken,
          string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(dataQualityScoreKey.AccountId, cancellationToken);

        var query = this.queryRequestBuilder.Build<DataQualityScoreRecord>(containerPath) as DataQualityScoreQuery;
        query.Timeout = DefaultTimeout;
        query.QueryByDimension = dataQualityScoreKey.QueryByDimension;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        await this.threadLockService.WaitAsync(LockName.DEHServerlessQueryLock);
        try
        {
            var list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);
            return new BaseBatchResults<DataQualityScoreEntity>
            {
                Results = list,
                ContinuationToken = continuationToken
            };
        }
        finally
        {
            this.threadLockService.Release(LockName.DEHServerlessQueryLock);
        }
    }

    private async Task<string> ConstructContainerPath(Guid accountId, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

        return $"{storageModel.GetDfsEndpoint()}/{storageModel.CatalogId}";
    }
}
