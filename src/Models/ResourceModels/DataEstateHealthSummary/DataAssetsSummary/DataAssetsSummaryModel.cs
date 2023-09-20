// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc/> 
public class DataAssetsSummaryModel : IDataAssetsSummaryModel
{
    /// <inheritdoc/> 
    [JsonProperty("totalCuratedDataAssetsCount")]
    public int TotalCuratedDataAssetsCount { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("totalCuratableDataAssetsCount")]
    public int TotalCuratableDataAssetsCount { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("totalNonCuratableDataAssetsCount")]
    public int TotalNonCuratableDataAssetsCount { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("dataAssetsDefaultTrendLink")]
    public string DataAssetsDefaultTrendLink { get; set; }

    /// <inheritdoc/> 
    [JsonProperty("dataAssetsLastRefreshDate")]
    public DateTime DataAssetsLastRefreshDate { get; set; }
}

