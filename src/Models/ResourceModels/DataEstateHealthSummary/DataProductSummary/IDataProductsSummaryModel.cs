// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the data product summary model
/// </summary>
public interface IDataProductsSummaryModel
{
    /// <summary>
    /// Total number of data products
    /// </summary>
    int TotalDataProductsCount { get; }

    /// <summary>
    /// Link to the data products default trend API
    /// </summary>
    string DataProductsDefaultTrendLink { get; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    DateTime DataProductsLastRefreshDate { get; }
}
