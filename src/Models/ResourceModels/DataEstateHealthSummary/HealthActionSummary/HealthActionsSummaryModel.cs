// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc/> 
public class HealthActionsSummaryModel : IHealthActionsSummaryModel
{
    /// <inheritdoc/>
    [JsonProperty("totalOpenActionsCount")]
    public int TotalOpenActionsCount { get; set; }

    /// <inheritdoc/>
    [JsonProperty("totalCompletedActionsCount")]
    public int TotalCompletedActionsCount { get; set; }

    /// <inheritdoc/>
    [JsonProperty("totalDismissedActionsCount")]
    public int TotalDismissedActionsCount { get; set; }

    /// <inheritdoc/>
    [JsonProperty("healthActionsDefaultTrendLink")]
    public string HealthActionsDefaultTrendLink { get; set; }

    /// <inheritdoc/>
    [JsonProperty("healthActionsLastRefreshDate")]
    public DateTime HealthActionsLastRefreshDate { get; set; }
}
