// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Defines operations for a tenant workspace.
/// </summary>
internal interface IWorkspaceCommand : IEntityCreateOperation<IWorkspaceRequest, Group>,
    IRetrieveEntityByIdOperation<IWorkspaceRequest, Group>,
    IEntityDeleteOperation<IWorkspaceRequest>
{
}
