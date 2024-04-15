// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using System.Threading;
using System.Threading.Tasks;
using ErrorCode = Common.ErrorCode;

/// <summary>
/// Creates a unique workspace. If the workspace already exists, it will be returned without modification.
/// </summary>
internal sealed class HealthWorkspaceCommand : IEntityCreateOperation<IWorkspaceContext, Group>,
    IRetrieveEntityByIdOperation<IWorkspaceContext, Group>,
    IEntityDeleteOperation<IWorkspaceContext>
{
    private readonly WorkspaceProvider workspaceCommand;
    private readonly IDataEstateHealthRequestLogger logger;

    public HealthWorkspaceCommand(WorkspaceProvider workspaceCommand, IDataEstateHealthRequestLogger logger)
    {
        this.workspaceCommand = workspaceCommand;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Group> Create(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("start to create workspace"))
        {
            IWorkspaceRequest workspaceRequest = new WorkspaceRequest()
            {
                AccountId = context.AccountId,
                ProfileId = context.ProfileId,
                WorkspaceName = OwnerNames.Health
            };

            return await this.workspaceCommand.Create(workspaceRequest, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<Microsoft.Purview.DataGovernance.Reporting.Common.DeletionResult> Delete(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("start to delete workspace"))
        {
            return await this.workspaceCommand.Delete(context.ProfileId, context.AccountId, OwnerNames.Health, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<Group> Get(IWorkspaceContext context, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed($"start to get workspace, account id: {context.AccountId}"))
        {
            return await this.workspaceCommand.Get(context.ProfileId, context.AccountId, OwnerNames.Health, cancellationToken) ?? throw new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.Workspace_NotFound.Code,
                ErrorCode.Workspace_NotFound.Message)
                .ToException();
        }
    }
}
