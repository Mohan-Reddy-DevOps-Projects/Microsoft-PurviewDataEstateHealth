// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// A health action model.
/// </summary>
public interface IHealthActionModel
{
    /// <summary>
    /// Health action Id
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Health action name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Health action description
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Health action owner detail
    /// </summary>
    OwnerContact OwnerContact { get; set; }

    /// <summary>
    /// Health control name
    /// </summary>
    string HealthControlName { get; set; }

    /// <summary>
    /// Health control state
    /// </summary>
    HealthControlState HealthControlState { get; set; }

    /// <summary>
    /// Health action created at 
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Health action last refreshed at 
    /// </summary>
    DateTime LastRefreshedAt { get; set; }

    /// <summary>
    /// Target details
    /// </summary>
    IEnumerable<TargetDetails> TargetDetailsList { get; set; }
}


