// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Status enumeration for SQL generation operations.
/// </summary>
public enum SqlGenerationWorkflowStatus
{
    /// <summary>
    /// SQL generation has not started or is in an unknown state.
    /// </summary>
    NotStarted,

    /// <summary>
    /// SQL generation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// SQL generation completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// SQL generation failed.
    /// </summary>
    Failed,

    /// <summary>
    /// SQL generation was cancelled.
    /// </summary>
    Cancelled
} 