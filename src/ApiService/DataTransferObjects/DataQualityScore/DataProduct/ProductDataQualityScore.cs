// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A quality scores class for product.
/// </summary>
public class ProductDataQualityScore
{
    /// <summary>
    /// Business DomainId 
    /// </summary>
    [ReadOnly(true)]
    public Guid BusinessDomainId { get; internal set; }

    /// <summary>
    /// Data Product Id
    /// </summary>
    [ReadOnly(true)]
    public Guid DataProductId { get; internal set; }

    /// <summary>
    /// Quality score 
    /// </summary>
    [ReadOnly(true)]
    public double QualityScore { get; internal set; }

    /// <summary>
    /// Last refreshed at timestamp
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshedAt { get; internal set; }
}
