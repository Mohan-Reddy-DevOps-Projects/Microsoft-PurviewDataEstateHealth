// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Health score property bag that should be persisted to persistence store.
/// </summary>
public class GovernanceAndQualityScoreProperties : HealthScoreProperties
{
    /// <summary>
    /// Actual Value.
    /// </summary>
    [JsonProperty("actualValue")]
    public float ActualValue { get; set; }

    /// <summary>
    /// Target Value.
    /// </summary>
    [JsonProperty("targetValue")]
    public float TargetValue { get; set; }

    /// <summary>
    /// Measure Unit.
    /// </summary>
    [JsonProperty("measureUnit")]
    public string MeasureUnit { get; set; }

    /// <summary>
    /// Performance Indicator Rules.
    /// </summary>
    [JsonProperty("performanceIndicatorRules")]
    public IEnumerable<PerformanceIndicatorRules> PerformanceIndicatorRules { get; set; }
}
