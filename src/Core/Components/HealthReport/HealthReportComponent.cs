// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;

/// <inheritdoc />
[Component(typeof(IHealthReportComponent), ServiceVersion.V1)]
internal class HealthReportComponent : BaseComponent<IHealthReportContext>, IHealthReportComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IReportCommand reportCommand;

    [Inject]
    private readonly HealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;
#pragma warning restore 649

    public HealthReportComponent(HealthReportContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc />
    public async Task<IHealthReportModel<HealthReportProperties>> Get(CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Get(this.Context, CancellationToken.None);
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        IReportRequest reportRequest = new ReportRequest()
        {
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            ReportId = this.Context.ReportId
        };
        Report report = await this.reportCommand.Get(reportRequest, cancellationToken);

        return new PowerBIHealthReportModel()
        {
            Properties = new PowerBIHealthReportProperties()
            {
                Category = null,
                LastRefreshedAt = DateTime.UtcNow,
                DatasetId = Guid.Parse(report.DatasetId),
                Description = report.Description,
                EmbedLink = report.EmbedUrl,
                Id = report.Id,
                Name = report.Name,
                ReportKind = HealthReportKind.PowerBIHealthReport,
                ReportStatus = HealthReportStatus.Active,
                ReportType = HealthReportType.System
            }
        };
    }
}
