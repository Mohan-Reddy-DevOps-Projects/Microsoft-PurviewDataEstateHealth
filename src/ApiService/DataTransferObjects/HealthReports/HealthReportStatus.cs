// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Report status.
/// </summary>
public enum HealthReportStatus
{
    /// <summary>
    /// Active health report
    /// </summary>
    Active = 1,

    /// <summary>
    /// Draft report.
    /// </summary>
    Draft
}
