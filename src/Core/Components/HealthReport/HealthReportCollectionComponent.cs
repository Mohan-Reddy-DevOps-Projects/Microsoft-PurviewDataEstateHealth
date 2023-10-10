// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Components.HealthReport;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;

/// <inheritdoc />
[Component(typeof(IHealthReportCollectionComponent), ServiceVersion.V1)]
internal class HealthReportCollectionComponent : BaseComponent<IHealthReportListContext>, IHealthReportCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private readonly IPowerBIService powerBIService;

    [Inject]
    private readonly HealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;
#pragma warning restore 649

    public HealthReportCollectionComponent(IHealthReportListContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc />
    public IHealthReportComponent ById(Guid id)
    {
        return this.ComponentRuntime.Resolve<IHealthReportComponent, IHealthReportContext>(
            this.contextFactory.CreateHealthReportContext(
                this.Context.Version,
                this.Context.Location,
                this.Context.AccountId,
                this.Context.TenantId,
                id),
            this.Context.Version.Numeric);
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IHealthReportModel<HealthReportProperties>>> Get(
        string skipToken = null,
        HealthReportKind? reportKind = null)
    {
        IProfileModel profile = await this.profileCommand.Get(this.Context, CancellationToken.None);
        IEnumerable<Report> reports = await this.GetReportsByProfile(profile, CancellationToken.None);

        IEnumerable<PowerBIHealthReportModel> values = reports.Select(x => new PowerBIHealthReportModel()
        {
            Properties = new PowerBIHealthReportProperties()
            {
                Category = null,
                LastRefreshedAt = DateTime.UtcNow,
                DatasetId = Guid.Parse(x.DatasetId),
                Description = x.Description,
                EmbedLink = x.EmbedUrl,
                Id = x.Id,
                Name = x.Name,
                ReportKind = HealthReportKind.PowerBIHealthReport,
                ReportStatus = HealthReportStatus.Active,
                ReportType = HealthReportType.System
            }
        });

        return new BaseBatchResults<IHealthReportModel<HealthReportProperties>>()
        {
            Results = values
        };
    }

    private async Task<IEnumerable<Report>> GetReportsByProfile(IProfileModel profile, CancellationToken cancellationToken)
    {
        IWorkspaceContext workspaceContext = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id,
        };
        Group workspace = await this.workspaceCommand.Get(workspaceContext, cancellationToken);
        IEnumerable<Report> reports = await this.GetReportsByWorkspace(profile.Id, workspace.Id, cancellationToken);

        return reports;
    }

    /// <summary>
    /// Get the list of reports available for the provided profile.
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IEnumerable<Report>> GetReportsByWorkspace(Guid profileId, Guid workspaceId, CancellationToken cancellationToken)
    {
        Reports reports = await this.powerBIService.GetReports(profileId, workspaceId, cancellationToken);

        return reports.Value.GroupBy(x => x.Name).Select(g => g.First());
    }

    /// <summary>
    /// Get the list of reports available for the provided profile.
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="reportId"></param>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Report> GetReportByWorkspace(Guid profileId, Guid workspaceId, Guid reportId, CancellationToken cancellationToken)
    {
        return await this.powerBIService.GetReport(profileId, workspaceId, reportId, cancellationToken);
    }
}
