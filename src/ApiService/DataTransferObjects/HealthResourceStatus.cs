// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Health resource status.
/// </summary>
public enum HealthResourceStatus
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
