// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;

internal class BusinessDomainRepository : IBusinessDomainRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    public BusinessDomainRepository(
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

    public async Task<IBatchResults<IBusinessDomainModel>> GetMultiple(
        BusinessDomainQueryCriteria criteria,
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
        string containerPath = await this.ConstructContainerPath(criteria.CatalogId.ToString(), criteria.AccountId, cancellationToken);

        BusinessDomainQuery query = this.queryRequestBuilder.Build<BusinessDomainRecord>(containerPath) as BusinessDomainQuery;

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        IList<BusinessDomainEntity> businessDomainEntitiesList = await this.queryExecutor.ExecuteAsync(query, cancellationToken);

        List<IBusinessDomainModel> businessDomainModelList = new();

        businessDomainModelList.AddRange(businessDomainEntitiesList.Select(businessDomainsEntity =>
            this.modelAdapterRegistry.AdapterFor<IBusinessDomainModel, BusinessDomainEntity>().ToModel(businessDomainsEntity)));

        return await Task.FromResult(new BaseBatchResults<IBusinessDomainModel>
        {
            Results = businessDomainModelList,
            ContinuationToken = null
        });
    }

    public IBusinessDomainRepository ByLocation(string location)
    {
        return new BusinessDomainRepository(
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
