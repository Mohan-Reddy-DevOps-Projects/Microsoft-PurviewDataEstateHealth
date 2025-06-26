// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using EntityModel.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using ServerlessPool.Queries.DataQualityOutput;
using ServerlessPool.Records.DataQualityOutput;
using System.Threading.Tasks;

internal class DataQualityOutputRepository : IDataQualityOutputRepository
{

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    private const int DefaultTimeout = 60 * 15;

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
        string containerPath = this.ConstructContainerPath(criteria.AccountStorageModel);

        DataQualityOutputQuery query = this.queryRequestBuilder.Build<DataQualityDataProductOutputRecord>(containerPath) as DataQualityOutputQuery;
        query.QueryPath = $"{containerPath}/{criteria.FolderPath}/*.parquet";
        query.Timeout = DefaultTimeout;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<DataQualityDataProductOutputEntity> list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return new BaseBatchResults<DataQualityDataProductOutputEntity>
        {
            Results = list,
            ContinuationToken = continuationToken
        };
    }

    public async Task<IBatchResults<DataQualityBusinessDomainOutputEntity>> GetMultipleForBusinessDomain(
          DataQualityOutputQueryCriteria criteria,
          CancellationToken cancellationToken,
          string continuationToken = null)
    {
        string containerPath = this.ConstructContainerPath(criteria.AccountStorageModel);

        var query = this.queryRequestBuilder.Build<DataQualityBusinessDomainOutputRecord>(containerPath) as DataQualityBusinessDomainOutputQuery;
        ArgumentNullException.ThrowIfNull(query);
        query.QueryPath = $"{containerPath}/{criteria.FolderPath}/*.parquet";
        query.Timeout = DefaultTimeout;
        var list = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return new BaseBatchResults<DataQualityBusinessDomainOutputEntity>
        {
            Results = list,
            ContinuationToken = continuationToken
        };
    }

    private string ConstructContainerPath(ProcessingStorageModel accountStorageModel)
    {
        ArgumentNullException.ThrowIfNull(accountStorageModel, nameof(accountStorageModel));
        return $"{accountStorageModel.GetDfsEndpoint()}/{accountStorageModel.CatalogId}";
    }
}
