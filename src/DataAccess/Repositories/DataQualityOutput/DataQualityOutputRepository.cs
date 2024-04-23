// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using System.Threading.Tasks;

internal class DataQualityOutputRepository : IDataQualityOutputRepository
{

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private const int DefaultTimeout = 60 * 30;
    private const int DefaultConnectTimeout = 60 * 30;

    public DataQualityOutputRepository(
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder)
    {
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
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
        query.ConnectTimeout = DefaultConnectTimeout;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<DataQualityDataProductOutputEntity> list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return new BaseBatchResults<DataQualityDataProductOutputEntity>
        {
            Results = list,
            ContinuationToken = continuationToken
        };
    }

    private async Task<string> ConstructContainerPath(Guid accountId, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

        return $"{storageModel.GetDfsEndpoint()}/{storageModel.CatalogId}";
    }
}
