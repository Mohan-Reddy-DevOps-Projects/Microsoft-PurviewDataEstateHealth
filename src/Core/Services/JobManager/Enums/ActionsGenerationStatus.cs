// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Status enumeration for actions generation workflow operations.
/// </summary>
public enum ActionsGenerationWorkflowStatus
{
    /// <summary>
    /// Actions generation has not started or is in an unknown state.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Actions generation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Actions generation completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Actions generation failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Actions generation was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Actions generation was skipped (e.g., no controls found).
    /// </summary>
    Skipped
} 