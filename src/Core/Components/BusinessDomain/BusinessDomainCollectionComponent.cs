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
        IQueryable<BusinessDomainEntity> businessDomainEntitiesList = await this.GetDataset(query, cancellationToken) as IQueryable<BusinessDomainEntity>;

        List<IBusinessDomainModel> businessDomainModelList = new();
        businessDomainModelList.AddRange(businessDomainEntitiesList.Select(businessDomainsEntity =>
            new BusinessDomainModel()
            {
                BusinessDomainId = businessDomainsEntity.BusinessDomainId,
                BusinessDomainName = businessDomainsEntity.BusinessDomainDisplayName
            }));

        return new BaseBatchResults<IBusinessDomainModel>
        {
            Results = businessDomainModelList,
            ContinuationToken = null
        };
    }

    private async Task<IQueryable> GetDataset<T>(ODataQueryOptions<T> query, CancellationToken cancellationToken)
    {
        return await this.datasetsComponent.GetDataset(query, cancellationToken);
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
