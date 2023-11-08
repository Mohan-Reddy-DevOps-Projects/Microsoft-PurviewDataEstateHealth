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

        IServerlessQueryRequest<DataEstateHealthSummaryRecord, DataEstateHealthSummaryEntity> query;

        if (summaryKey == null || !summaryKey.DomainId.HasValue)
        {
            query = this.queryRequestBuilder.Build<DataEstateHealthSummaryRecord, DataEstateHealthSummaryEntity>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereClause(QueryConstants.DataEstateHealthSummaryColumnNamesForKey.BusinessDomainId, Guid.Empty.ToString());
            });
        }
        else
        {
            query = this.queryRequestBuilder.Build<DataEstateHealthSummaryRecord, DataEstateHealthSummaryEntity>(containerPath, clauseBuilder =>
            {
                clauseBuilder.WhereClause(QueryConstants.DataEstateHealthSummaryColumnNamesForKey.BusinessDomainId, summaryKey.DomainId.Value.ToString());
            });
        }

        var dataEstateHealthSummaryEntity = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        return await Task.FromResult(this.modelAdapterRegistry
            .AdapterFor<IDataEstateHealthSummaryModel, DataEstateHealthSummaryEntity>()
            .ToModel(dataEstateHealthSummaryEntity.FirstOrDefault()));
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
