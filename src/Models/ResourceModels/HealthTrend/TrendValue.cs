// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Trend value class
/// </summary>
public class TrendValue
{
    /// <summary>
    /// Health TrendValue capture date
    /// </summary>
    [JsonProperty("captureDate")]
    public DateTime CaptureDate { get; set; }

    /// <summary>
    /// Health TrendValue value
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }
}
