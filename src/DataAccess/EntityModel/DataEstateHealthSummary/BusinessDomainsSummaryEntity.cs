// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class BusinessDomainsSummaryEntity : BaseEntity
{
    public BusinessDomainsSummaryEntity()
    {
    }

    public BusinessDomainsSummaryEntity(BusinessDomainsSummaryEntity entity)
    {
        this.TotalBusinessDomainsCount = entity.TotalBusinessDomainsCount;
        this.BusinessDomainsFilterListLink = entity.BusinessDomainsFilterListLink;
        this.BusinessDomainsTrendLink = entity.BusinessDomainsTrendLink;
        this.BusinessDomainsLastRefreshDate = entity.BusinessDomainsLastRefreshDate;
    }

    [JsonProperty("totalBusinessDomainsCount")]
    public int TotalBusinessDomainsCount { get; set; }

    [JsonProperty("businessDomainsFilterListLink")]
    public string BusinessDomainsFilterListLink { get; set; }

    [JsonProperty("businessDomainsTrendLink")]
    public string BusinessDomainsTrendLink { get; set; }

    [JsonProperty("businessDomainsLastRefreshDate")]
    public DateTime BusinessDomainsLastRefreshDate { get; set; }
}
