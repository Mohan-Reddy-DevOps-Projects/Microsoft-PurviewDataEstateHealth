// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Linq;
using System.Threading.Tasks;
using global::Microsoft.Azure.Purview.DataEstateHealth.Common;
using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.BaseModels;
using global::Microsoft.DGP.ServiceBasics.Components;
using global::Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Purview.DataGovernance.DataLakeAPI.Entities;

[Component(typeof(IBusinessDomainCollectionComponent), ServiceVersion.V1)]
internal class BusinessDomainCollectionComponent : BaseComponent<IBusinessDomainListContext>, IBusinessDomainCollectionComponent
{

#pragma warning disable 649

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;

    [Inject]
    private readonly IDatasetsComponent datasetsComponent;

    [Inject]
    private readonly IODataModelProvider modelProvider;
#pragma warning disable 649

    public BusinessDomainCollectionComponent(IBusinessDomainListContext context, int version) : base(context, version)
    {
    }

    public async Task<IBatchResults<IBusinessDomainModel>> Get(CancellationToken cancellationToken,
        string skipToken = null)
    {
        ODataQueryOptions<BusinessDomainEntity> query = this.GetOptions();
        await using SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
        Func<IQueryable<BusinessDomainEntity>> x = () => query.ApplyTo(context.BusinessDomains.AsQueryable()) as IQueryable<BusinessDomainEntity>;
        IQueryable<BusinessDomainEntity> businessDomainEntitiesList = this.datasetsComponent.GetDataset(x) as IQueryable<BusinessDomainEntity>;

        List<IBusinessDomainModel> businessDomainModelList = new();
        BaseBatchResults<IBusinessDomainModel> result = new()
        {
            Results = businessDomainModelList,
            ContinuationToken = null
        };
        if (businessDomainEntitiesList != null)
        {
            businessDomainModelList.AddRange(businessDomainEntitiesList.Select(businessDomainsEntity =>
                new BusinessDomainModel()
                {
                    BusinessDomainId = businessDomainsEntity.BusinessDomainId,
                    BusinessDomainName = businessDomainsEntity.BusinessDomainDisplayName
                }));
        }

        return result;
    }

    private ODataQueryOptions<BusinessDomainEntity> GetOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntityType entityType = model.FindDeclaredType("Microsoft.Azure.Purview.DataEstateHealth.DataAccess.BusinessDomainEntity") as IEdmEntityType;
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("BusinessDomain");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(BusinessDomainEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }
}
