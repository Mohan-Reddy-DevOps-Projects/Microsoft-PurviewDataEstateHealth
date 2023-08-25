// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Enumerates different states while performing deletion
/// </summary>
public enum DeletionStatus
{
    /// <summary>
    /// Unknown state, could indicate failure
    /// </summary>
    Unknown,

    /// <summary>
    /// Resource to delete was not found
    /// </summary>
    ResourceNotFound,

    /// <summary>
    /// Deletion was completed
    /// </summary>
    Deleted,

    /// <summary>
    /// Deletion request was accepted
    /// </summary>
    Accepted
}
