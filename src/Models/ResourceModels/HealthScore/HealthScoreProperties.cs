// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Health score property bag that should be persisted to persistence store.
/// </summary>
[JsonConverter(typeof(HealthScorePropertiesConverter))]
public class HealthScoreProperties
{
    /// <summary>
    /// Score kind.
    /// </summary>
    [JsonProperty("scoreKind")]
    public HealthScoreKind ScoreKind { get; set; }

    /// <summary>
    /// Score name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Score description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Report Id.
    /// </summary>
    [JsonProperty("reportId")]
    public Guid ReportId { get; set; }
}
