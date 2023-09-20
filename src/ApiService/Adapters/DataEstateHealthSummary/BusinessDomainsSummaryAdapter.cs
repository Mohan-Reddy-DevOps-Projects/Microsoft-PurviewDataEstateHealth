// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Collections.Generic;
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
        var businessDomainsListDTO = new List<BusinessDomain>();
        foreach (var domain in model.BusinessDomainsList)
        {
            businessDomainsListDTO.Add(new BusinessDomain
            {
                BusinessDomainId = domain.BusinessDomainId,
                BusinessDomainName = domain.BusinessDomainName,
            });
        }

        return new BusinessDomainsSummary
        {
            TotalBusinessDomainsCount = model.TotalBusinessDomainsCount,
            BusinessDomainsList = businessDomainsListDTO,
            BusinessDomainsDefaultTrendLink = model.BusinessDomainsDefaultTrendLink,
            LastRefreshDate = model.BusinessDomainsLastRefreshDate
        };
    }

    /// <inheritdoc />
    public override IBusinessDomainsSummaryModel ToModel(BusinessDomainsSummary businessDomainSummaryDto)
    {
        var businessDomainsListModel = new List<Models.BusinessDomain>();
        foreach (var domain in businessDomainSummaryDto.BusinessDomainsList)
        {
            businessDomainsListModel.Add(new Models.BusinessDomain
            {
                BusinessDomainId = domain.BusinessDomainId,
                BusinessDomainName = domain.BusinessDomainName,
            });
        }

        return new BusinessDomainsSummaryModel
        {
            TotalBusinessDomainsCount = businessDomainSummaryDto.TotalBusinessDomainsCount,
            BusinessDomainsList = businessDomainsListModel,
            BusinessDomainsDefaultTrendLink = businessDomainSummaryDto.BusinessDomainsDefaultTrendLink,
            BusinessDomainsLastRefreshDate = businessDomainSummaryDto.LastRefreshDate
        };
    }
}
