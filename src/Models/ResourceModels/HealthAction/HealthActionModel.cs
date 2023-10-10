// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <inheritdoc />
public class HealthActionModel : IHealthActionModel
{
    /// <inheritdoc />
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <inheritdoc />
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <inheritdoc />
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <inheritdoc />
    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    /// <inheritdoc />
    [JsonProperty("healthControlName")]
    public string HealthControlName { get; set; }

    /// <inheritdoc />
    [JsonProperty("healthControlState")]
    public HealthControlState HealthControlState { get; set; }

    /// <inheritdoc />
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    [JsonProperty("lastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }

    /// <inheritdoc />
    [JsonProperty("targetDetails")]
    public IEnumerable<TargetDetails> TargetDetailsList { get; set; }
}
