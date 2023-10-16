// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc/> 
public class BusinessDomainsSummaryModel : IBusinessDomainsSummaryModel
{
    /// <inheritdoc/> 
    [JsonProperty("totalBusinessDomainsCount")]
    public int TotalBusinessDomainsCount { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("businessDomainsFilterListLink")]
    public string BusinessDomainsFilterListLink { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("businessDomainsTrendLink")]
    public string BusinessDomainsTrendLink { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("businessDomainsLastRefreshDate")]
    public DateTime BusinessDomainsLastRefreshDate { get; set; }
}
