// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Curation score property bag that should be persisted to persistence store.
/// </summary>
public class DataCurationScoreProperties : HealthScoreProperties
{
    /// <summary>
    /// Total curated count.
    /// </summary>
    [JsonProperty("totalCuratedCount")]
    public string TotalCuratedCount { get; set; }

    /// <summary>
    /// Total can be curated count.
    /// </summary>
    [JsonProperty("totalCanBeCuratedCount")]
    public string TotalCanBeCuratedCount { get; set; }

    /// <summary>
    /// Total cannot be curated count.
    /// </summary>
    [JsonProperty("totalCannotBeCuratedCount")]
    public string TotalCannotBeCuratedCount { get; set; }
}
