// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// A score threshold class.
/// </summary>
public class PerformanceIndicatorRules
{
    /// <summary>
    /// Rule order.
    /// </summary>
    [JsonProperty("ruleOrder")]
    public int RuleOrder { get; set; }

    /// <summary>
    /// Minimum threshold value.
    /// </summary>
    [JsonProperty("minValue")]
    public int MinValue { get; set; }

    /// <summary>
    /// Maximum threshold value.
    /// </summary>
    [JsonProperty("maxValue")]
    public int MaxValue { get; set; }

    /// <summary>
    /// Threshold display text.
    /// </summary>
    [JsonProperty("displayText")]
    public string DisplayText { get; set; }

    /// <summary>
    /// Threshold default color.
    /// </summary>
    [JsonProperty("defaultColor")]
    public string DefaultColor { get; set; }
}
