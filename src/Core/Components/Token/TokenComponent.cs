// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Data;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;

[Component(typeof(ITokenComponent), ServiceVersion.V1)]
internal sealed class TokenComponent : BaseComponent<ITokenContext>, ITokenComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IPowerBIService powerBIService;

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
        if (tokenModel == null || tokenModel.ReportIds == null || tokenModel.ReportIds.Count == 0 || tokenModel.DatasetIds == null || tokenModel.DatasetIds.Count == 0)
        {
            return await this.GetReadToken(cancellationToken);
        }

        return await this.GetEditToken(tokenModel, cancellationToken);
    }

    private async Task<EmbedToken> GetReadToken(CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Get(this.Context, cancellationToken);
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        Reports reports = await this.powerBIService.GetReports(profile.Id, workspace.Id, cancellationToken);

        IEnumerable<Guid> datasetIds = reports.Value.Select(x => Guid.Parse(x.DatasetId));
        IEnumerable<Guid> reportIds = reports.Value.Select(x => x.Id);

        return await this.GenerateEmbeddedToken(datasetIds, reportIds, profile.Id, false, cancellationToken);
    }

    private async Task<EmbedToken> GetEditToken(TokenModel tokenModel, CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Get(this.Context, cancellationToken);
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        Reports reports = await this.powerBIService.GetReports(profile.Id, workspace.Id, cancellationToken);
        HashSet<string> excludedNames = HealthReportNames.System;
        IEnumerable<Report> filteredReports = reports.Value.Where(report => !excludedNames.Contains(report.Name));
        HashSet<Guid> filteredIds = new(filteredReports.Select(x => x.Id));
        IEnumerable<Guid> editableReportIds = filteredIds.Intersect(tokenModel.ReportIds).Distinct();
        IEnumerable<Guid> editableDatasetIds = filteredReports.Select(x => Guid.Parse(x.DatasetId)).Intersect(tokenModel.DatasetIds).Distinct();

        return await this.GenerateEmbeddedToken(editableDatasetIds, editableReportIds, profile.Id, true, cancellationToken);
    }

    /// <summary>
    /// Get the embedded
    /// </summary>
    /// <param name="datasets"></param>
    /// <param name="reports"></param>
    /// <param name="profileId"></param>
    /// <param name="enableWriterPermission"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<EmbedToken> GenerateEmbeddedToken(IEnumerable<Guid> datasets, IEnumerable<Guid> reports, Guid profileId, bool enableWriterPermission, CancellationToken cancellationToken)
    {
        EmbeddedTokenRequest request = new()
        {
            DatasetIds = datasets.ToArray(),
            ReportIds = reports.ToArray(),
            LifetimeInMinutes = 60,
            ProfileId = profileId,
            EnableWriterPermission = enableWriterPermission
        };

        return await this.powerBIService.GenerateEmbeddedToken(request, cancellationToken);
    }
}
