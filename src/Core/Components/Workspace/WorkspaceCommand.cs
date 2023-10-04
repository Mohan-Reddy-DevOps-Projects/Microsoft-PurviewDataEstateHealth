// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;

internal sealed class WorkspaceCommand : IWorkspaceCommand
{
    private readonly IDataEstateHealthLogger logger;
    private readonly IPowerBIService powerBiService;

    public WorkspaceCommand(IDataEstateHealthLogger logger, IPowerBIService powerBiService)
    {
        this.logger = logger;
        this.powerBiService = powerBiService;
    }

    /// <inheritdoc/>
    public async Task<Group> Create(IWorkspaceRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);
        Group workspace = await this.Get(requestContext, cancellationToken);
        if (workspace != null)
        {
            return workspace;
        }
        try
        {
            string workspaceName = GetWorkspaceName(requestContext);
            return await this.powerBiService.CreateWorkspace(requestContext.ProfileId, workspaceName, cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to create workspace={requestContext}", ex);
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.Workspace_CreateFailed.Code, $"Failed to create workspace.").ToException();
        }
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(IWorkspaceRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);
        Group workspace = await this.Get(requestContext, cancellationToken);
        if (workspace == null)
        {
            return new DeletionResult()
            {
                DeletionStatus = DeletionStatus.ResourceNotFound
            };
        }
        try
        {
            await this.powerBiService.DeleteWorkspace(requestContext.ProfileId, workspace.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            // TODO: catch not found exception in case the profile is already deleted
            this.logger.LogError($"Failed to delete profile={requestContext}", ex);
            return new DeletionResult()
            {
                DeletionStatus = DeletionStatus.Unknown
            };
        }

        return new DeletionResult()
        {
            DeletionStatus = DeletionStatus.Deleted,
            JobId = Guid.NewGuid().ToString(),
            Location = string.Empty
        };
    }

    /// <inheritdoc/>
    public async Task<Group> Get(IWorkspaceRequest requestContext, CancellationToken cancellationToken)
    {
        Validate(requestContext);

        try
        {
            string workspaceName = GetWorkspaceName(requestContext);
            Groups workspaces = await this.powerBiService.GetWorkspaces(requestContext.ProfileId, cancellationToken, filter: $"name eq '{workspaceName}'");
            return workspaces.Value.FirstOrDefault();
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to get workspace={requestContext}", ex);
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.Workspace_GetFailed.Code, $"Failed to get workspace.").ToException();
        }
    }

    private static string GetWorkspaceName(IWorkspaceRequest requestContext) => $"{requestContext.WorkspaceName}-{requestContext.AccountId}";

    private static void Validate(IWorkspaceRequest requestContext)
    {
        if (string.IsNullOrEmpty(requestContext.WorkspaceName))
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceName.").ToException();
        }
        if (requestContext.AccountId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing AccountId.").ToException();
        }
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
    }
}
