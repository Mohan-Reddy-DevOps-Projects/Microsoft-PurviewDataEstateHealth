// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class DataAssetsSummaryEntity
{
    public DataAssetsSummaryEntity()
    {
    }

    public DataAssetsSummaryEntity(DataAssetsSummaryEntity entity)
    {
        this.DataAssetsTrendLink = entity.DataAssetsTrendLink;
        this.DataAssetsLastRefreshDate = entity.DataAssetsLastRefreshDate;
        this.TotalCuratableDataAssetsCount = entity.TotalCuratableDataAssetsCount;
        this.TotalCuratedDataAssetsCount = entity.TotalCuratedDataAssetsCount;
        this.TotalNonCuratableDataAssetsCount = entity.TotalNonCuratableDataAssetsCount;
    }

    [JsonProperty("totalCuratedDataAssetsCount")]
    public int TotalCuratedDataAssetsCount { get; set; }

    [JsonProperty("totalCuratableDataAssetsCount")]
    public int TotalCuratableDataAssetsCount { get; set; }

    [JsonProperty("totalNonCuratableDataAssetsCount")]
    public int TotalNonCuratableDataAssetsCount { get; set; }

    [JsonProperty("dataAssetsTrendLink")]
    public string DataAssetsTrendLink { get; set; }

    [JsonProperty("dataAssetsLastRefreshDate")]
    public DateTime DataAssetsLastRefreshDate { get; set; }
}
