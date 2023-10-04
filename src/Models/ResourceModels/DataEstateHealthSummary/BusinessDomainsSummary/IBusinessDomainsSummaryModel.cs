// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the business domain summary model
/// </summary>
public interface IBusinessDomainsSummaryModel
{
    /// <summary>
    /// Total number of business domains
    /// </summary>
     int TotalBusinessDomainsCount { get; }

    /// <summary>
    /// List of all of the business domains 
    /// </summary>
    IEnumerable<BusinessDomain> BusinessDomainsList { get; }

    /// <summary>
    /// Link to the business domains  API 
    /// </summary>
    string BusinessDomainsTrendLink { get; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    DateTime BusinessDomainsLastRefreshDate { get; }
}
