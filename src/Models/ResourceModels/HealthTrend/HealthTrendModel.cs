// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <inheritdoc />
public class HealthTrendModel : IHealthTrendModel
{
    /// <inheritdoc />
    [JsonProperty("kind")]
    public TrendKind Kind { get; set; }

    /// <inheritdoc />
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <inheritdoc />
    [JsonProperty("duration")]
    public string Duration { get; set; }

    /// <inheritdoc />
    [JsonProperty("unit")]
    public string Unit { get; set; }

    /// <inheritdoc />
    [JsonProperty("trendValuesList")]
    public IEnumerable<TrendValue> TrendValuesList { get; set; }
}
