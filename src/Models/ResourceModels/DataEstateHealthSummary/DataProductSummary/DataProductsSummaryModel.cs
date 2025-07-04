﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc/> 
public class DataProductsSummaryModel : IDataProductsSummaryModel
{
    /// <summary>
    /// Total number of data products
    /// </summary>
    [JsonProperty("totalDataProductsCount")]
    public int TotalDataProductsCount { get; set; }

    /// <summary>
    /// Link to the data products default  API
    /// </summary>
    [JsonProperty("dataProductsTrendLink")]
    public string DataProductsTrendLink { get; set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [JsonProperty("dataProductsLastRefreshDate")]
    public DateTime DataProductsLastRefreshDate { get; set; }
}
