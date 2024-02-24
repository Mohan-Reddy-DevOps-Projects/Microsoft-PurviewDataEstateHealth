// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Workspace context
/// </summary>
public interface IWorkspaceContext : IRootComponentContext
{
    /// <summary>
    /// The profile id to access the workspace.
    /// </summary>
    Guid ProfileId { get; }
}
