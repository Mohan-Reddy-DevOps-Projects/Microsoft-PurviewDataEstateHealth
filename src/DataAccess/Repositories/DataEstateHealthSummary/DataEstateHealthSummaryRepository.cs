// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

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

        var dataEstateHealthSummaryEntity = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return await Task.FromResult(this.modelAdapterRegistry
            .AdapterFor<IDataEstateHealthSummaryModel, DataEstateHealthSummaryEntity>()
            .ToModel(dataEstateHealthSummaryEntity.FirstOrDefault() as DataEstateHealthSummaryEntity));
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
