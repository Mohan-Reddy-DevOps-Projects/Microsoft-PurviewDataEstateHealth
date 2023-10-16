// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A health governance score and quality score property bag.
/// </summary>
public class GovernanceAndQualityScoreProperties
{
    /// <summary>
    /// Actual Value.
    /// </summary>
    [ReadOnly(true)]
    public int ActualValue { get; internal set; }

    /// <summary>
    /// Target Value.
    /// </summary>
    [ReadOnly(true)]
    public int TargetValue { get; set; }

    /// <summary>
    /// Measure Unit.
    /// </summary>
    [ReadOnly(true)]
    public string MeasureUnit { get; set; }

    /// <summary>
    /// Performance indicator rules.
    /// </summary>
    [ReadOnly(true)]
    public IEnumerable<PerformanceIndicatorRules> PerformanceIndicatorRules { get; set; }
}
