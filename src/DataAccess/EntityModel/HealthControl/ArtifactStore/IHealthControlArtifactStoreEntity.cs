// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

internal interface IHealthControlArtifactStoreEntity
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

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("parentControlId")]
    public Guid ParentControlId { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("isCompositeControl")]
    public bool IsCompositeControl { get; set; }

    [JsonProperty("controlType")]
    public HealthResourceType ControlType { get; set; }

    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    [JsonProperty("targetScore")]
    public int TargetScore { get; set; }

    [JsonProperty("scoreUnit")]
    public string ScoreUnit { get; set; }

    [JsonProperty("healthStatus")]
    public string HealthStatus { get; set; }

    [JsonProperty("controlStatus")]
    public HealthResourceStatus ControlStatus { get; set; }

    [JsonProperty("startsAt")]
    public DateTime StartsAt { get; set; }

    [JsonProperty("endsAt")]
    public DateTime EndsAt { get; set; }

    [JsonProperty("trendUrl")]
    public string TrendUrl { get; set; }
}
