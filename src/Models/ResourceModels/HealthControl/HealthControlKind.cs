// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health control kind.
/// </summary>
public enum HealthControlKind
{
    /// <summary>
    /// DataGovernance Control.
    /// </summary>
    DataGovernance = 1,

    /// <summary>
    /// Data quality control.
    /// </summary>
    DataQuality,
}
