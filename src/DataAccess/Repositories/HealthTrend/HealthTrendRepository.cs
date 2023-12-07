// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.ServerlessPool.Records.HealthTrend;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

internal class HealthTrendRepository : IHealthTrendRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    public HealthTrendRepository(
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

    public async Task<IHealthTrendModel> GetSingle(HealthTrendKey healthTrendKey, CancellationToken cancellationToken)
    {
        string containerPath = await this.ConstructContainerPath(healthTrendKey.CatalogId.ToString(), healthTrendKey.AccountId, cancellationToken);

        IServerlessQueryRequest<BaseRecord, BaseEntity> query;

        string selectClause = QueryConstants.HealthTrendKindToColumnName[healthTrendKey.TrendKind];
        string today = DateTime.UtcNow.Date.ToString();
        string thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-1 * HealthTrendKey.TrendDuration).ToString();

        if (healthTrendKey == null || !healthTrendKey.DomainId.HasValue)
        {
            query = this.queryRequestBuilder.Build<HealthTrendRecordForAllBusinessDomains>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereBetweenClause(QueryConstants.HealthTrendsColumnNamesForKey.LastRefreshedAt, thirtyDaysAgo, today);
            }, selectClause) as HealthTrendsQueryForAllBusinessDomains;
        } else
        {
            query = this.queryRequestBuilder.Build<HealthTrendRecord>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereBetweenClause(QueryConstants.HealthTrendsColumnNamesForKey.LastRefreshedAt, thirtyDaysAgo, today);
                clauseBuilder.AndClause(QueryConstants.HealthTrendsColumnNamesForKey.BusinessDomainId, healthTrendKey.DomainId.ToString());
            }, selectClause) as HealthTrendsQuery;
        }

        IList<BaseEntity> healthTrendEntity = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return this.modelAdapterRegistry
            .AdapterFor<IHealthTrendModel, HealthTrendEntity>()
            .ToModel(healthTrendEntity.FirstOrDefault() as HealthTrendEntity);
    }

    public IHealthTrendRepository ByLocation(string location)
    { 
        return new HealthTrendRepository(
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
