// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

internal class HealthActionEntity
{
    public HealthActionEntity()
    {
    }

    public HealthActionEntity(HealthActionEntity entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.OwnerContact = entity.OwnerContact;
        this.HealthControlState = entity.HealthControlState;
        this.HealthControlName = entity.HealthControlName;
        this.CreatedAt = entity.CreatedAt;
        this.LastRefreshedAt = entity.LastRefreshedAt;
        this.TargetDetailsList = entity.TargetDetailsList;
    }

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    [JsonProperty("healthControlName")]
    public string HealthControlName { get; set; }

    [JsonProperty("healthControlState")]
    public HealthControlState HealthControlState { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("lastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }

    [JsonProperty("targetDetails")]
    public IEnumerable<TargetDetails> TargetDetailsList { get; set; }
}
