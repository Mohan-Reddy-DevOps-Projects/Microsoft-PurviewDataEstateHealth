// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Status enumeration for controls workflow operations.
/// </summary>
public enum ControlsWorkflowStatus
{
    /// <summary>
    /// Controls workflow has not started or is in an unknown state.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Controls workflow is currently being submitted.
    /// </summary>
    Submitting,

    /// <summary>
    /// Controls workflow is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// Controls workflow completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Controls workflow failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Controls workflow was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Controls workflow timed out.
    /// </summary>
    TimedOut
} 