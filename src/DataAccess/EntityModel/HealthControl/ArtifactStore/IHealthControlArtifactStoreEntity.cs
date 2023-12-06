// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

/// <summary>
/// Interface for health control artifact store entity.
/// </summary>
public interface IHealthControlArtifactStoreEntity
{
    /// <summary>
    /// Object id of the entity
    /// </summary>
    [JsonProperty("objectId")]
    public Guid ObjectId { get; set; }

    /// <summary>
    /// Version of the entity
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Date/Time this entity was created.
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date/Time this entity was modified.
    /// </summary>
    [JsonProperty("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Parent control id.
    /// </summary>
    [JsonProperty("parentControlId")]
    public Guid ParentControlId { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Is composite control.
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
    /// Starts at.
    /// </summary>
    [JsonProperty("startsAt")]
    public DateTime StartsAt { get; set; }

    /// <summary>
    /// Ends at.
    /// </summary>
    [JsonProperty("endsAt")]
    public DateTime EndsAt { get; set; }

    /// <summary>
    /// Trend url.
    /// </summary>
    [JsonProperty("trendUrl")]
    public string TrendUrl { get; set; }
}
