// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Newtonsoft.Json;

internal class DataProductsSummaryEntity
{
    public DataProductsSummaryEntity()
    {
    }

    public DataProductsSummaryEntity(DataProductsSummaryEntity entity)
    {
        this.DataProductsTrendLink = entity.DataProductsTrendLink;
        this.DataProductsLastRefreshDate = entity.DataProductsLastRefreshDate;
        this.TotalDataProductsCount = entity.TotalDataProductsCount;
    }

    [JsonProperty("totalDataProductsCount")]
    public int TotalDataProductsCount { get; set; }

    [JsonProperty("dataProductsTrendLink")]
    public string DataProductsTrendLink { get; set; }

    [JsonProperty("dataProductsLastRefreshDate")]
    public DateTime DataProductsLastRefreshDate { get; set; }
}
