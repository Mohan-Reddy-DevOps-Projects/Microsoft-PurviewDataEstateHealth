// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;

internal class HealthScoreRepository : IHealthScoreRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    public HealthScoreRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
        this.location = location;
    }

    public async Task<IBatchResults<IHealthScoreModel<HealthScoreProperties>>> GetMultiple(
         HealthScoreKey healthScoreKey,
         CancellationToken cancellationToken,
         string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(healthScoreKey.CatalogId.ToString(), healthScoreKey.AccountId, cancellationToken);

        IServerlessQueryRequest<BaseRecord, BaseEntity> query;

        if (healthScoreKey == null || !healthScoreKey.BusinessDomainId.HasValue)
        {
            query = this.queryRequestBuilder.Build<HealthScoreRecordForAllBusinessDomains>(containerPath) as HealthScoresQueryForAllBusinessDomains;
        }
        else
        {
            query = this.queryRequestBuilder.Build<HealthScoreRecord>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereClause(QueryConstants.HealthScoresColumnNamesForKey.BusinessDomainId, healthScoreKey.BusinessDomainId.Value.ToString());
            }) as HealthScoresQuery;
        }

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        var healthScoreEntitiesList = await this.queryExecutor.ExecuteAsync(query, cancellationToken) as IList<HealthScoreEntity>;

        var healthScoreModelList = new List<IHealthScoreModel<HealthScoreProperties>>();
        foreach (var healthScoresEntity in healthScoreEntitiesList)
        {
            healthScoreModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScoreEntity>()
                                              .ToModel(healthScoresEntity));
        }

        return await Task.FromResult(new BaseBatchResults<IHealthScoreModel<HealthScoreProperties>>
        {
            Results = healthScoreModelList,
            ContinuationToken = null
        });
    }

    public IHealthScoreRepository ByLocation(string location)
    {
        return new HealthScoreRepository(
            this.modelAdapterRegistry,
            this.processingStorageManager,
            this.queryExecutor,
            this.queryRequestBuilder,
            location);
    }

    private async Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

        return $"{storageModel.GetDfsEndpoint()}/{containerName}";
    }
}
