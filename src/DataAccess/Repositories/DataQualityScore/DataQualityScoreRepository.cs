// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

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

    private const int DefaultTimeout = 60 * 60 * 1000;

    public DataQualityScoreRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
    }

    public async Task<IBatchResults<DataQualityScoreEntity>> GetMultiple(
          DataQualityScoreKey dataQualityScoreKey,
          CancellationToken cancellationToken,
          string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(dataQualityScoreKey.AccountId, cancellationToken);

        var query = this.queryRequestBuilder.Build<DataQualityScoreRecord>(containerPath,
            (x) =>
            {
                if (!string.IsNullOrEmpty(dataQualityScoreKey.Dimension))
                {
                    x.WhereClause("QualityDimension", dataQualityScoreKey.Dimension);
                }
                else
                {
                    x.WhereClause("QualityDimension", "NULL", QueryConstants.SQLOperator.Is);
                }
            }) as DataQualityScoreQuery;
        query.Timeout = DefaultTimeout;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<DataQualityScoreEntity> list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return new BaseBatchResults<DataQualityScoreEntity>
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
