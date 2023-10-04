// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A data asset data transfer object.
/// </summary>
public class DataAssetsSummary
{
    /// <summary>
    /// Total number of curated data assets
    /// </summary>
    [ReadOnly(true)]
    public int TotalCuratedDataAssetsCount { get; internal set; }

    /// <summary>
    /// Total number of curatable data assets
    /// </summary>
    [ReadOnly(true)]
    public int TotalCuratableDataAssetsCount { get; internal set; }

    /// <summary>
    /// Total number of non-curatable data assets
    /// </summary>
    [ReadOnly(true)]
    public int TotalNonCuratableDataAssetsCount { get; internal set; }

    /// <summary>
    /// Link to the data assets  API
    /// </summary>
    [ReadOnly(true)]
    public string DataAssetsTrendLink { get; internal set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshDate { get; internal set; }
}

