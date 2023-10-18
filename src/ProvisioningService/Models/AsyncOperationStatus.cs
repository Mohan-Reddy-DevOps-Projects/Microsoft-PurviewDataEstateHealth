// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

/// <summary>
/// Possible statuses of an operation done by a PartnerService.
/// </summary>
public enum AsyncOperationStatus
{
    /// <summary>
    /// Non-terminal state for an operation that hasn't started
    /// </summary>
    NotStarted,

    /// <summary>
    /// Non-terminal state for an operation that is currently running
    /// </summary>
    Running,

    /// <summary>
    /// Terminal state for an operation that succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// Terminal state for an operation that failed
    /// </summary>
    Failed,
}
