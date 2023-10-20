// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

[JsonConverter(typeof(HealthControlEntityConverter))]
internal class HealthControlEntity
{
    public HealthControlEntity()
    {
    }

    public HealthControlEntity(HealthControlEntity entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsCompositeControl = entity.IsCompositeControl;
        this.ControlType = entity.ControlType;
        this.OwnerContact = entity.OwnerContact;
        this.CurrentScore = entity.CurrentScore;
        this.TargetScore = entity.TargetScore;
        this.ScoreUnit = entity.ScoreUnit;
        this.HealthStatus = entity.HealthStatus;
        this.ControlStatus = entity.ControlStatus;
        this.CreatedAt = entity.CreatedAt;
        this.StartsAt = entity.StartsAt;
        this.EndsAt = entity.EndsAt;
        this.TrendUrl = entity.TrendUrl;
        this.BusinessDomainsListLink = entity.BusinessDomainsListLink;
    }

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("kind")]
    public HealthControlKind Kind { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("isCompositeControl")]
    public bool IsCompositeControl { get; set; }

    [JsonProperty("controlType")]
    public HealthResourceType ControlType { get; set; }

    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    [JsonProperty("currentScore")]
    public int CurrentScore { get; set; }

    [JsonProperty("targetScore")]
    public int TargetScore { get; set; }

    [JsonProperty("scoreUnit")]
    public string ScoreUnit { get; set; }

    [JsonProperty("healthStatus")]
    public string HealthStatus { get; set; }

    [JsonProperty("controlStatus")]
    public HealthResourceStatus ControlStatus { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("startsAt")]
    public DateTime StartsAt { get; set; }

    [JsonProperty("endsAt")]
    public DateTime EndsAt { get; set; }

    [JsonProperty("trendUrl")]
    public string TrendUrl { get; set; }

    [JsonProperty("businessDomainsListLink")]
    public string BusinessDomainsListLink { get; set; }
}
