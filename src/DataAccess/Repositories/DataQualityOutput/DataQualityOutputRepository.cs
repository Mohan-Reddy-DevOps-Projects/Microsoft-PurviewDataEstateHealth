// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using System.Threading.Tasks;

internal class DataQualityOutputRepository : IDataQualityOutputRepository
{

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private readonly IThreadLockService threadLockService;

    private const int DefaultTimeout = 60 * 15;

    public DataQualityOutputRepository(
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         IThreadLockService threadLockService)
    {
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
        this.threadLockService = threadLockService;
    }

    public async Task<IBatchResults<DataQualityDataProductOutputEntity>> GetMultiple(
          DataQualityOutputQueryCriteria criteria,
          CancellationToken cancellationToken,
          string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(new Guid(criteria.AccountId), cancellationToken);

        DataQualityOutputQuery query = this.queryRequestBuilder.Build<DataQualityDataProductOutputRecord>(containerPath) as DataQualityOutputQuery;
        query.QueryPath = $"{containerPath}/{criteria.FolderPath}/*.parquet";
        query.Timeout = DefaultTimeout;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        this.threadLockService.WaitOne(LockName.DEHServerlessQueryLock);
        try
        {
            IList<DataQualityDataProductOutputEntity> list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

            return new BaseBatchResults<DataQualityDataProductOutputEntity>
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
