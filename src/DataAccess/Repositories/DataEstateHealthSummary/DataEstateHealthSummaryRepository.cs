// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;

internal class DataEstateHealthSummaryRepository : IDataEstateHealthSummaryRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    public DataEstateHealthSummaryRepository(
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

    public async Task<IDataEstateHealthSummaryModel> GetSingle(SummaryKey summaryKey, CancellationToken cancellationToken)
    {
        string containerPath = await this.ConstructContainerPath(summaryKey.CatalogId.ToString(), summaryKey.AccountId, cancellationToken);

        IServerlessQueryRequest<BaseRecord, BaseEntity> query;

        if (summaryKey == null || !summaryKey.DomainId.HasValue)
        {
            query = this.queryRequestBuilder.Build<DataEstateHealthSummaryRecordForAllBusinessDomains>(containerPath) as DataEstateHealthSummaryQueryForAllBusinessDomains;
        }
        else
        {
            query = this.queryRequestBuilder.Build<DataEstateHealthSummaryRecord>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereClause(QueryConstants.DataEstateHealthSummaryColumnNamesForKey.BusinessDomainId, summaryKey.DomainId.Value.ToString());
            }) as DataEstateHealthSummaryQuery;
        }

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<BaseEntity> dataEstateHealthSummaryEntity = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return this.modelAdapterRegistry
            .AdapterFor<IDataEstateHealthSummaryModel, DataEstateHealthSummaryEntity>()
            .ToModel(dataEstateHealthSummaryEntity.FirstOrDefault() as DataEstateHealthSummaryEntity);
    }

    public async Task<IBatchResults<IHealthActionModel>> GetMultiple(
          HealthActionKey healthActionKey,
          CancellationToken cancellationToken,
          string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(healthActionKey.CatalogId.ToString(), healthActionKey.AccountId, cancellationToken);

        HealthActionsQuery query;

        if (healthActionKey == null || !healthActionKey.BusinessDomainId.HasValue)
        {
            query = this.queryRequestBuilder.Build<HealthActionsRecord>(containerPath) as HealthActionsQuery;

        }
        else
        {
            query = this.queryRequestBuilder.Build<HealthActionsRecord>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereClause(QueryConstants.HealthActionColumnNamesForKey.BusinessDomainId, healthActionKey.BusinessDomainId.Value.ToString());
            }) as HealthActionsQuery;
        }

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<HealthActionEntity> healthActionEntititiesList = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        List<IHealthActionModel> healthActionModelList = healthActionEntititiesList.Select(healthActionsEntity =>
        this.modelAdapterRegistry.AdapterFor<IHealthActionModel, HealthActionEntity>().ToModel(healthActionsEntity))
            .ToList();

        return new BaseBatchResults<IHealthActionModel>
        {
            Results = healthActionModelList,
            ContinuationToken = null
        };
    }

    public IDataEstateHealthSummaryRepository ByLocation(string location)
    {
        return new DataEstateHealthSummaryRepository(
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
