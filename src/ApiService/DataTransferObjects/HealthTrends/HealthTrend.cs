// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// A health trend data transfer object.
/// </summary>
public class HealthTrend
{
    /// <summary>
    /// Health trend kind
    /// </summary>
    [Required]
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("kind")]
    public TrendKind Kind { get; internal set; }

    /// <summary>
    /// Health trend description
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; internal set; }

    /// <summary>
    /// Health trend duration
    /// </summary>
    [ReadOnly(true)]
    public string Duration { get; internal set; }

    /// <summary>
    /// Health trend unit
    /// </summary>
    [ReadOnly(true)]
    public string Unit { get; internal set; }

    /// <summary>
    /// Health trend values
    /// </summary>
    [ReadOnly(true)]
    public IEnumerable<TrendValue> TrendValuesList { get; internal set; }
}
