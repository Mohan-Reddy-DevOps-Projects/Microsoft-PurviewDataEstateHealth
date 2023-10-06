// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api.Models;

[Component(typeof(IPartnerNotificationComponent), ServiceVersion.V1)]
internal sealed class PartnerNotificationComponent : BaseComponent<IPartnerNotificationContext>, IPartnerNotificationComponent
{
#pragma warning disable 649
    [Inject]
    private readonly HealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;

    [Inject]
    private readonly IDatasetCommand datasetCommand;

    [Inject]
    private readonly IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration;

    [Inject]
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

#pragma warning restore 649

    public PartnerNotificationComponent(IPartnerNotificationContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateNotification(CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Create(this.Context, cancellationToken);
        IWorkspaceContext context = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id
        };
        Group workspace = await this.workspaceCommand.Create(context, cancellationToken);
        string owner = "health";
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, owner, cancellationToken);
        if (powerBICredential == null)
        {
            // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
            powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, owner);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
        }

        string datasetName = "CDMC";
        IDatasetRequest datasetRequest = new DatasetRequest()
        {
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            AccountId = context.AccountId,
            DatabaseName = $"{owner}_1",
            DatabaseSchema = $"{owner}-{context.AccountId}",
            Server = this.serverlessPoolConfiguration.Value.SqlEndpoint.Split(';').First(),
            DatasetContainer = "powerbi",
            DatasetFileName = $"{datasetName}.pbix",
            DatasetName = datasetName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential
        };
        Dataset dataset = await this.datasetCommand.Create(datasetRequest, cancellationToken);
    }
}
