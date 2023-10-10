// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Report kind.
/// </summary>
public enum HealthReportKind
{
    /// <summary>
    /// PowerBI health report
    /// </summary>
    PowerBIHealthReport = 1,

    /// <summary>
    /// Legacy report.
    /// </summary>
    LegacyHealthReport
}
