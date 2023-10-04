// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A Data Product data transfer object.
/// </summary>
public class DataProductsSummary
{
    /// <summary>
    /// Total number of data products
    /// </summary>
    [ReadOnly(true)]
    public int TotalDataProductsCount { get; internal set; }

    /// <summary>
    /// Link to the data products default  API
    /// </summary>
    [ReadOnly(true)]
    public string DataProductsTrendLink { get; internal set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshDate { get; internal set; }
}
