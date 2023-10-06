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
    public async Task<EmbedToken> Get(CancellationToken cancellationToken)
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

        return await this.GenerateEmbeddedToken(datasetIds, reportIds, profile.Id, cancellationToken);
    }

    /// <summary>
    /// Get the embedded
    /// </summary>
    /// <param name="datasets"></param>
    /// <param name="reports"></param>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<EmbedToken> GenerateEmbeddedToken(IEnumerable<Guid> datasets, IEnumerable<Guid> reports, Guid profileId, CancellationToken cancellationToken)
    {
        EmbeddedTokenRequest request = new()
        {
            DatasetIds = datasets.ToArray(),
            ReportIds = reports.ToArray(),
            LifetimeInMinutes = 60,
            ProfileId = profileId
        };

        return await this.powerBIService.GenerateEmbeddedToken(request, cancellationToken);
    }
}
