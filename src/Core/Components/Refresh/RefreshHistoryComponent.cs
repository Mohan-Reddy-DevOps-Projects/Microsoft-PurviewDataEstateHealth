// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;

[Component(typeof(IRefreshHistoryComponent), ServiceVersion.V1)]
internal sealed class RefreshHistoryComponent : BaseComponent<IRefreshHistoryContext>, IRefreshHistoryComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IPowerBIService powerBIService;

    [Inject]
    private readonly HealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;

#pragma warning restore 649

    public RefreshHistoryComponent(IRefreshHistoryContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task<IBatchResults<Refresh>> Get(CancellationToken cancellationToken, string skipToken = null)
    {
        if (this.Context.Top < 1)
        {
            throw new ServiceError(ErrorCategory.InputError, ServiceErrorCode.InvalidField.Code, "Top must be greater than 0.").ToException();
        }

        IProfileModel profile = await this.profileCommand.Get(this.Context.AccountId, cancellationToken);
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);

        Refreshes response = await this.powerBIService.GetRefreshHistory(profile.Id, workspace.Id, this.Context.DatasetId, cancellationToken, top: this.Context.Top);

        return new BaseBatchResults<Refresh>()
        {
            Results = response.Value,
            ContinuationToken = null,
        };
    }
}
