// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Refresh lookup model
/// </summary>
public sealed class RefreshLookup
{
    /// <summary>
    /// Dataset Id.
    /// </summary>
    public Guid DatasetId { get; init; }

    /// <summary>
    /// Refresh Request Id.
    /// </summary>
    public Guid RefreshRequestId { get; init; }

    /// <summary>
    /// Workspace Id.
    /// </summary>
    public Guid WorkspaceId { get; init; }

    /// <summary>
    /// Profile Id.
    /// </summary>
    public Guid ProfileId { get; init; }
}

