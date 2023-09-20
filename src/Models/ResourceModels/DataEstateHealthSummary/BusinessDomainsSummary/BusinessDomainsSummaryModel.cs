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
    [JsonProperty("businessDomainsList")]
    public IList<BusinessDomain> BusinessDomainsList { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("businessDomainsDefaultTrendLink")]
    public string BusinessDomainsDefaultTrendLink { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("businessDomainsLastRefreshDate")]
    public DateTime BusinessDomainsLastRefreshDate { get; set; }
}
