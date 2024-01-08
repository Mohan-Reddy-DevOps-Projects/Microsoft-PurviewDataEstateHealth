// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;
/// <summary>
/// Defines the data asset summary model
/// </summary>
public interface IDataAssetsSummaryModel
{
    /// <summary>
    /// Total number of curated data assets
    /// </summary>
    int TotalCuratedDataAssetsCount { get; }

    /// <summary>
    /// Total number of curatable data assets
    /// </summary>
    int TotalCuratableDataAssetsCount { get; }

    /// <summary>
    /// Total number of non-curatable data assets
    /// </summary>
    int TotalNonCuratableDataAssetsCount { get;}

    /// <summary>
    /// Link to the data assets  API
    /// </summary>
    string DataAssetsTrendLink { get; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    DateTime DataAssetsLastRefreshDate { get; }
}
