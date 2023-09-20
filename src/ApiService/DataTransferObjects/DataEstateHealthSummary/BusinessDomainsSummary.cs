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
    /// List of all of the business domains 
    /// </summary>
    [ReadOnly(true)]
    public IList<BusinessDomain> BusinessDomainsList { get; internal set; }

    /// <summary>
    /// Link to the business domains trend API 
    /// </summary>
    [ReadOnly(true)]
    public string BusinessDomainsDefaultTrendLink { get; internal set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshDate { get; internal set; }
}
