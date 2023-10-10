// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Report Type.
/// </summary>
public enum HealthReportType
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
