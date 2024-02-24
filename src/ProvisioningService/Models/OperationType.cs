// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

/// <summary>
/// Enumerates different operation types
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Resource to create or update
    /// </summary>
    CreateOrUpdate,

    /// <summary>
    /// Resource to delete was not found
    /// </summary>
    Delete,

    /// <summary>
    /// Deletion was completed
    /// </summary>
    SoftDelete
}
