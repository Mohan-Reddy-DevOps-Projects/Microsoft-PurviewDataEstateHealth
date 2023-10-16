// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for BusinessDomainsSummaryEntity to BusinessDomainsSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IBusinessDomainsSummaryModel), typeof(BusinessDomainsSummaryEntity))]
internal class BusinessDomainsSummaryEntityAdapter : BaseModelAdapter<IBusinessDomainsSummaryModel, BusinessDomainsSummaryEntity>
{
    public override BusinessDomainsSummaryEntity FromModel(IBusinessDomainsSummaryModel model)
    {
        return new BusinessDomainsSummaryEntity()
        {
            TotalBusinessDomainsCount = model.TotalBusinessDomainsCount,
            BusinessDomainsFilterListLink = model.BusinessDomainsFilterListLink,
            BusinessDomainsTrendLink = model.BusinessDomainsTrendLink,
            BusinessDomainsLastRefreshDate = model.BusinessDomainsLastRefreshDate,
        };
    }

    public override IBusinessDomainsSummaryModel ToModel(BusinessDomainsSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new BusinessDomainsSummaryModel()
        {
            TotalBusinessDomainsCount = entity.TotalBusinessDomainsCount,
            BusinessDomainsFilterListLink = entity.BusinessDomainsFilterListLink,
            BusinessDomainsTrendLink = entity.BusinessDomainsTrendLink,
            BusinessDomainsLastRefreshDate = entity.BusinessDomainsLastRefreshDate,
        };
    }
}
