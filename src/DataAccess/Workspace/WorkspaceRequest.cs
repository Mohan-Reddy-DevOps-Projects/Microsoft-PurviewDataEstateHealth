// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Text.Json;

/// <summary>
/// Workspace request.
/// </summary>
public sealed class WorkspaceRequest : IWorkspaceRequest
{
    /// <summary>
    /// Constructor
    /// </summary>
    public WorkspaceRequest() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="other"></param>
    public WorkspaceRequest(IWorkspaceRequest other)
    {
        this.AccountId = other.AccountId;
        this.ProfileId = other.ProfileId;
        this.WorkspaceName = other.WorkspaceName;
    }

    /// <inheritdoc/>
    public Guid AccountId { get; set; }

    /// <inheritdoc/>
    public Guid ProfileId { get; set; }

    /// <inheritdoc/>
    public string WorkspaceName { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
