// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the health action summary model.
/// </summary>
public interface IHealthActionsSummaryModel
{
    /// <summary>
    /// Total number of open health actions
    /// </summary>
    int TotalOpenActionsCount { get; }

    /// <summary>
    /// Total number of completed health actions
    /// </summary>
    int TotalCompletedActionsCount { get; }

    /// <summary>
    /// Total number of dismissed health actions
    /// </summary>
    int TotalDismissedActionsCount { get; }

    /// <summary>
    /// Link to the health actions  API
    /// </summary>
    string HealthActionsTrendLink { get; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    DateTime HealthActionsLastRefreshDate { get; }
}
