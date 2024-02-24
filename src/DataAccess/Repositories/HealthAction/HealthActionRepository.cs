// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;

internal class HealthActionRepository : IHealthActionRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    public HealthActionRepository(
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

    public IHealthActionRepository ByLocation(string location)
    {
        return new HealthActionRepository(
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
