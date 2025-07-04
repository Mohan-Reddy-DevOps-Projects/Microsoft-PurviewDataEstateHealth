﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <inheritdoc />
[Component(typeof(IHealthReportComponent), ServiceVersion.V1)]
internal class HealthReportComponent : BaseComponent<IHealthReportContext>, IHealthReportComponent
{
#pragma warning disable 649
    [Inject]
    private readonly ReportProvider reportCommand;

    [Inject]
    private readonly IHealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;
#pragma warning restore 649

    public HealthReportComponent(HealthReportContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc />
    public async Task<IHealthReportModel<HealthReportProperties>> Get(CancellationToken cancellationToken)
    {
        ProfileKey profileKey = new(this.Context.AccountId);
        IProfileModel profile = await this.profileCommand.Get(profileKey, CancellationToken.None);
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        Report report = await this.reportCommand.Get(profile.Id, workspace.Id, this.Context.ReportId, cancellationToken);

        return new PowerBIHealthReportModel()
        {
            Properties = new PowerBIHealthReportProperties()
            {
                Category = null,
                DatasetId = Guid.Parse(report.DatasetId),
                Description = report.Description,
                EmbedLink = report.EmbedUrl,
                Id = report.Id,
                Name = report.Name,
                ReportKind = HealthReportKind.PowerBIHealthReport,
                ReportStatus = HealthResourceStatus.Active,
                ReportType = HealthReportNames.System.Contains(report.Name) ? HealthResourceType.System : HealthResourceType.Custom
            }
        };
    }
}
