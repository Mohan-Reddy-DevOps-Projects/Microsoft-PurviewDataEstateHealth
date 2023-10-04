// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

internal class BusinessDomainsSummaryEntity
{
    public BusinessDomainsSummaryEntity()
    {
    }

    public BusinessDomainsSummaryEntity(BusinessDomainsSummaryEntity entity)
    {
        this.TotalBusinessDomainsCount = entity.TotalBusinessDomainsCount;
        this.BusinessDomainsList = entity.BusinessDomainsList;
        this.BusinessDomainsTrendLink = entity.BusinessDomainsTrendLink;
        this.BusinessDomainsLastRefreshDate = entity.BusinessDomainsLastRefreshDate;
    }

    [JsonProperty("totalBusinessDomainsCount")]
    public int TotalBusinessDomainsCount { get; set; }

    [JsonProperty("businessDomainsList")]
    public IEnumerable<BusinessDomain> BusinessDomainsList { get; set; }

    [JsonProperty("businessDomainsTrendLink")]
    public string BusinessDomainsTrendLink { get; set; }

    [JsonProperty("businessDomainsLastRefreshDate")]
    public DateTime BusinessDomainsLastRefreshDate { get; set; }
}
