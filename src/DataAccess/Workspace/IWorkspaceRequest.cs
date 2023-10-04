// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// The workspace request.
/// </summary>
public interface IWorkspaceRequest
{
    /// <summary>
    /// The profile id.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// The workspace name.
    /// </summary>
    string WorkspaceName { get; }

    /// <summary>
    /// The account id.
    /// </summary>
    Guid AccountId { get; }
}
