// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Health control property bag that should be persisted to metadata store.
/// </summary>
[JsonConverter(typeof(HealthControlPropertiesConverter))]
public class HealthControlProperties
{
    /// <summary>
    /// Control Id.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Control kind.
    /// </summary>
    [JsonProperty("kind")]
    public HealthControlKind Kind { get; set; }

    /// <summary>
    /// Health control name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Health control name description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Flag to detect if its a parent control.
    /// </summary>
    [JsonProperty("isCompositeControl")]
    public bool IsCompositeControl { get; set; }

    /// <summary>
    /// Control type.
    /// </summary>
    [JsonProperty("controlType")]
    public HealthResourceType ControlType { get; set; }

    /// <summary>
    /// Owner contact.
    /// </summary>
    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    /// <summary>
    /// Current score.
    /// </summary>
    [JsonProperty("currentScore")]
    public int CurrentScore { get; set; }

    /// <summary>
    /// Target score.
    /// </summary>
    [JsonProperty("targetScore")]
    public int TargetScore { get; set; }

    /// <summary>
    /// Score unit.
    /// </summary>
    [JsonProperty("scoreUnit")]
    public string ScoreUnit { get; set; }

    /// <summary>
    /// Health status.
    /// </summary>
    [JsonProperty("healthStatus")]
    public string HealthStatus { get; set; }

    /// <summary>
    /// Control status.
    /// </summary>
    [JsonProperty("controlStatus")]
    public HealthResourceStatus ControlStatus { get; set; }

    /// <summary>
    /// Control created at.
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Control starts at.
    /// </summary>
    [JsonProperty("startsAt")]
    public DateTime StartsAt { get;  set; }

    /// <summary>
    /// Control ends at.
    /// </summary>
    [JsonProperty("endsAt")]
    public DateTime EndsAt { get;  set; }

    /// <summary>
    /// Trend link.
    /// </summary>
    [JsonProperty("trendUrl")]
    public string TrendUrl { get; set; }

    /// <summary>
    /// Business domains list link.
    /// </summary>
    [JsonProperty("businessDomainsListLink")]
    public string BusinessDomainsListLink { get; set; }
}
