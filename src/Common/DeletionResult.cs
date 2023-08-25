// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Enumerates different states while performing deletion
/// </summary>
public class DeletionResult
{
    /// <summary>
    /// Resource to delete was not found
    /// </summary>
    public DeletionStatus DeletionStatus { get; set; }

    /// <summary>
    /// Location of the resource
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Job id of the job queues to preform deletion.
    /// Can be null if no job was queued
    /// </summary>
    public string JobId { get; set; }
}
