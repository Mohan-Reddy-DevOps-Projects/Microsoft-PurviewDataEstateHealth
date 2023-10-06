// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Creates a unique workspace. If the workspace already exists, it will be returned without modification.
/// </summary>
internal sealed class HealthWorkspaceCommand : IEntityCreateOperation<IWorkspaceContext, Group>,
    IRetrieveEntityByIdOperation<IWorkspaceContext, Group>,
    IEntityDeleteOperation<IWorkspaceContext>
{
    private const string HealthWorkspaceName = "health";
    private readonly IWorkspaceCommand workspaceCommand;

    public HealthWorkspaceCommand(IWorkspaceCommand workspaceCommand)
    {
        this.workspaceCommand = workspaceCommand;
    }

    /// <inheritdoc/>
    public async Task<Group> Create(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        IWorkspaceRequest workspaceRequest = new WorkspaceRequest()
        {
            AccountId = context.AccountId,
            ProfileId = context.ProfileId,
            WorkspaceName = HealthWorkspaceName
        };

        return await this.workspaceCommand.Create(workspaceRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        IWorkspaceRequest workspaceRequest = new WorkspaceRequest()
        {
            AccountId = context.AccountId,
            ProfileId = context.ProfileId,
            WorkspaceName = HealthWorkspaceName
        };

        return await this.workspaceCommand.Delete(workspaceRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Group> Get(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        IWorkspaceRequest workspaceRequest = new WorkspaceRequest()
        {
            AccountId = context.AccountId,
            ProfileId = context.ProfileId,
            WorkspaceName = HealthWorkspaceName
        };

        return await this.workspaceCommand.Get(workspaceRequest, cancellationToken) ?? throw new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.Workspace_NotFound.Code,
                ErrorCode.Workspace_NotFound.Message)
                .ToException();
    }
}
