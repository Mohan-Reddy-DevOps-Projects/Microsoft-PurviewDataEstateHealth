// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// A class that holds properties to indicate performance.
/// </summary>
public class PerformanceIndicatorRules
{
    /// <summary>
    /// Rule order.
    /// </summary>
    [ReadOnly(true)]
    public int RuleOrder { get; internal set; }

    /// <summary>
    /// Minimum threshold value.
    /// </summary>
    [Required]
    public int MinValue { get; set; }

    /// <summary>
    /// Maximum threshold value.
    /// </summary>
    [Required]
    public int MaxValue { get; set; }

    /// <summary>
    /// Threshold display text.
    /// </summary>
    [Required]
    public string DisplayText { get; set; }

    /// <summary>
    /// Threshold default color.
    /// </summary>
    [Required]
    public string DefaultColor { get; set; }
}
