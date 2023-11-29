// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Health controls property bag
/// </summary>
public class HealthControlProperties
{
    /// <summary>
    /// Health control name.
    /// </summary>
    [ReadOnly(true)]
    public string Name  { get; internal set; }

    /// <summary>
    /// Health control name description.
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; internal set; }

    /// <summary>
    /// Flag to detect if its a composite control.
    /// </summary>
    [ReadOnly(true)]
    public bool IsCompositeControl { get; internal set; }

    /// <summary>
    /// Control type.
    /// </summary>
    [ReadOnly(true)]
    public HealthResourceType ControlType { get; internal set; }

    /// <summary>
    /// Owner contact.
    /// </summary>
    [ReadOnly(true)]
    public OwnerContact OwnerContact { get; internal set; }

    /// <summary>
    /// Current score.
    /// </summary>
    [ReadOnly(true)]
    public double CurrentScore { get; internal set; }

    /// <summary>
    /// Target score.
    /// </summary>
    [ReadOnly(true)]
    public int TargetScore { get; internal set; }

    /// <summary>
    /// Score unit.
    /// </summary>
    [ReadOnly(true)]
    public string ScoreUnit { get; internal set; }

    /// <summary>
    /// Health status.
    /// </summary>
    [ReadOnly(true)]
    public string HealthStatus { get; internal set; }

    /// <summary>
    /// Control status.
    /// </summary>
    [ReadOnly(true)]
    public HealthResourceStatus ControlStatus { get; internal set; }

    /// <summary>
    /// Control created at.
    /// </summary>
    [ReadOnly(true)]
    public DateTime CreatedAt { get; internal set; }

    /// <summary>
    /// Control starts at.
    /// </summary>
    [ReadOnly(true)]
    public DateTime StartsAt { get; internal set; }

    /// <summary>
    /// Control ends at.
    /// </summary>
    [ReadOnly(true)]
    public DateTime EndsAt { get; internal set; }

    /// <summary>
    /// Trend link.
    /// </summary>
    [ReadOnly(true)]
    public string TrendUrl { get; internal set; }

    /// <summary>
    /// Business domains list link.
    /// </summary>
    [ReadOnly(true)]
    public string BusinessDomainsListLink { get; internal set; }
}
