// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// A health trend model
/// </summary>
public interface IHealthTrendModel
{
    /// <summary>
    /// Health trend kind
    /// </summary>
    TrendKind Kind { get; set; }

    /// <summary>
    /// Health trend description
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Health trend duration
    /// </summary>
    string Duration { get; set; }

    /// <summary>
    /// Health trend unit
    /// </summary>
    string Unit { get; set; }

    /// <summary>
    /// Health trend delta
    /// </summary>
    int Delta { get; set; }

    /// <summary>
    /// Health trend values
    /// </summary>
    IEnumerable<TrendValue> TrendValuesList { get; set; }
}
