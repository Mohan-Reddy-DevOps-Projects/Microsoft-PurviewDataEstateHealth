// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Business Domain Summary Adapter
/// </summary>
[ModelAdapter(typeof(IBusinessDomainsSummaryModel), typeof(BusinessDomainsSummary))]
public class BusinessDomainsSummaryAdapter : BaseModelAdapter<IBusinessDomainsSummaryModel, BusinessDomainsSummary>
{
    /// <inheritdoc />
    public override BusinessDomainsSummary FromModel(IBusinessDomainsSummaryModel model)
    {
        return new BusinessDomainsSummary
        {
            TotalBusinessDomainsCount = model.TotalBusinessDomainsCount,
            BusinessDomainsFilterListLink = model.BusinessDomainsFilterListLink,
            BusinessDomainsTrendLink = model.BusinessDomainsTrendLink,
            LastRefreshDate = model.BusinessDomainsLastRefreshDate
        };
    }

    /// <inheritdoc />
    public override IBusinessDomainsSummaryModel ToModel(BusinessDomainsSummary businessDomainSummaryDto)
    { 
        return new BusinessDomainsSummaryModel
        {
            TotalBusinessDomainsCount = businessDomainSummaryDto.TotalBusinessDomainsCount,
            BusinessDomainsFilterListLink = businessDomainSummaryDto.BusinessDomainsFilterListLink,
            BusinessDomainsTrendLink = businessDomainSummaryDto.BusinessDomainsTrendLink,
            BusinessDomainsLastRefreshDate = businessDomainSummaryDto.LastRefreshDate
        };
    }
}
