// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A health action data transfer object.
/// </summary>
public class HealthActionsSummary
{
    /// <summary>
    /// Total number of open health actions
    /// </summary>
    [ReadOnly(true)]
    public int TotalOpenActionsCount { get; internal set; }

    /// <summary>
    /// Total number of completed health actions
    /// </summary>
    [ReadOnly(true)]
    public int TotalCompletedActionsCount { get; internal set; }

    /// <summary>
    /// Total number of dismissed health actions
    /// </summary>
    [ReadOnly(true)]
    public int TotalDismissedActionsCount { get; internal set; }

    /// <summary>
    /// Link to the health actions  API
    /// </summary>
    [ReadOnly(true)]
    public string HealthActionsTrendLink { get; internal set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshDate { get; internal set; }
}
