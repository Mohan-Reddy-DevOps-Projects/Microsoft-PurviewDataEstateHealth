// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Newtonsoft.Json;

internal class BusinessDomainRepository : IBusinessDomainRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly string location;

    const string businessDomainsListJson = @"[{""businessDomainName"":""Finance"",""businessDomainId"":""8cae4ce6-85e0-4816-9579-d40840109e0b""},{""businessDomainName"":""Legal"",""businessDomainId"":""10247fce-26ab-45f8-a82d-af37def43669""},{""businessDomainName"":""Marketing"",""businessDomainId"":""ef35b13c-40bd-4aa8-87da-58eb1100af6f""},{""businessDomainName"":""IT"",""businessDomainId"":""bed33e7a-2d2f-4843-b3d0-2cf9fee4b7e9""}]";

    public BusinessDomainRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.location = location;
    }

    public async Task<IBatchResults<IBusinessDomainModel>> GetMultiple(
        CancellationToken cancellationToken,
        string continuationToken = null)
    {
        var businessDomainEntitiesList = JsonConvert.DeserializeObject<IList<BusinessDomainEntity>>(businessDomainsListJson);

        var businessDomainModelList = new List<IBusinessDomainModel>();
        foreach (var businessDomainsEntity in businessDomainEntitiesList)
        {
            businessDomainModelList.Add(this.modelAdapterRegistry
                               .AdapterFor<IBusinessDomainModel, BusinessDomainEntity>()
                                              .ToModel(businessDomainsEntity));
        }

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
            location);
    }
}
