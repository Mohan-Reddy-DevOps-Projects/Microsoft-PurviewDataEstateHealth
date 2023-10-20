// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Health resource Type.
/// </summary>
public enum HealthResourceType
{
    /// <summary>
    /// System health report.
    /// </summary>
    System = 1,

    /// <summary>
    /// Custom health report.
    /// </summary>
    Custom
}
