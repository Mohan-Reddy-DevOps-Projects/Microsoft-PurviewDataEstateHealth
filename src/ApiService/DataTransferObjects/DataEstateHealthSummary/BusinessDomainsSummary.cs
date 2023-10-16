// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A Business Domain data transfer object.
/// </summary>
public class BusinessDomainsSummary
{
    /// <summary>
    /// Total number of business domains
    /// </summary>
    [ReadOnly(true)]
    public int TotalBusinessDomainsCount { get; internal set; }

    /// <summary>
    /// Business domains link
    /// </summary>
    [ReadOnly(true)]
    public string BusinessDomainsFilterListLink { get; internal set; }

    /// <summary>
    /// Link to the business domains  API 
    /// </summary>
    [ReadOnly(true)]
    public string BusinessDomainsTrendLink { get; internal set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshDate { get; internal set; }
}
