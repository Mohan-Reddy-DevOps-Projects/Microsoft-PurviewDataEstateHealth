// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Data;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;

[Component(typeof(ITokenComponent), ServiceVersion.V1)]
internal sealed class TokenComponent : BaseComponent<ITokenContext>, ITokenComponent
{
#pragma warning disable 649
    [Inject]
    private readonly ReportProvider reportProvider;

    [Inject]
    private readonly TokenProvider tokenProvider;

    [Inject]
    private readonly HealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;

#pragma warning restore 649

    public TokenComponent(ITokenContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task<EmbedToken> Get(TokenModel tokenModel, CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Get(this.Context.AccountId, cancellationToken);
        TokenModel request;
        if (tokenModel == null || tokenModel.ReportIds == null || tokenModel.ReportIds.Count == 0 || tokenModel.DatasetIds == null || tokenModel.DatasetIds.Count == 0)
        {
            request = await this.GetReadToken(profile, cancellationToken);
        }
        else
        {
            request = await this.GetEditToken(profile, tokenModel, cancellationToken);
        }

        return await this.tokenProvider.Get(profile, request, cancellationToken);
    }

    private async Task<TokenModel> GetReadToken(IProfileModel profile, CancellationToken cancellationToken)
    {
        WorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        Reports reports = await this.reportProvider.List(profile.Id, workspace.Id, cancellationToken);

        IEnumerable<Guid> datasetIds = reports.Value.Select(x => Guid.Parse(x.DatasetId));
        IEnumerable<Guid> reportIds = reports.Value.Select(x => x.Id);

        return new TokenModel()
        {
            DatasetIds = datasetIds.ToHashSet(),
            ReportIds = reportIds.ToHashSet(),
            IsEdit = false
        };
    }

    private async Task<TokenModel> GetEditToken(IProfileModel profile, TokenModel tokenModel, CancellationToken cancellationToken)
    {
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        Reports reports = await this.reportProvider.List(profile.Id, workspace.Id, cancellationToken);
        HashSet<string> excludedNames = HealthReportNames.System;
        IEnumerable<Report> filteredReports = reports.Value.Where(report => !excludedNames.Contains(report.Name));
        HashSet<Guid> filteredIds = new(filteredReports.Select(x => x.Id));
        IEnumerable<Guid> editableReportIds = filteredIds.Intersect(tokenModel.ReportIds).Distinct();
        IEnumerable<Guid> editableDatasetIds = filteredReports.Select(x => Guid.Parse(x.DatasetId)).Intersect(tokenModel.DatasetIds).Distinct();

        return new TokenModel()
        {
            DatasetIds = editableDatasetIds.ToHashSet(),
            ReportIds = editableReportIds.ToHashSet(),
            IsEdit = true
        };
    }
}
