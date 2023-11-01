// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
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
    private readonly IDatabaseManagementService databaseManagementService;

/*  [Inject]
    private readonly IReportCommand reportCommand;

    [Inject]
    private readonly IDatasetCommand datasetCommand;

    [Inject]
    private readonly IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration;

    [Inject]
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

    [Inject]
    private readonly ICapacityAssignment capacityAssignment;*/

#pragma warning restore 649

    public PartnerNotificationComponent(IPartnerNotificationContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateNotification(AccountServiceModel account, CancellationToken cancellationToken)
    {
        await this.databaseManagementService.Initialize(this.Context.AccountId, cancellationToken);

        try
        {
            IProfileModel profile = await this.profileCommand.Create(this.Context, cancellationToken);
            IWorkspaceContext context = new WorkspaceContext(this.Context)
            {
                ProfileId = profile.Id
            };
            Group workspace = await this.workspaceCommand.Create(context, cancellationToken);
        }
        catch (Exception)
        {
        }

        /*
        await this.capacityAssignment.AssignWorkspace(profile.Id, workspace.Id, cancellationToken);
        string owner = "health";
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, owner, cancellationToken);
        if (powerBICredential == null)
        {
            // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
            powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, owner);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
        }

        string datasetName = "CDMC_Report";
        IDatasetRequest datasetRequest = new DatasetRequest()
        {
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            DatasetContainer = "powerbi",
            DatasetFileName = $"{datasetName}.pbix",
            DatasetName = datasetName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential
            
        };

        string sharedDatasetName = "CDMC_Dataset";
        IDatasetRequest sharedDatasetRequest = new DatasetRequest()
        {
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            DatasetContainer = "powerbi",
            DatasetFileName = $"{sharedDatasetName}.pbix",
            DatasetName = sharedDatasetName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential,
            SkipReport = true,
            Parameters = new Dictionary<string, string>()
            {
                { "SERVER", this.serverlessPoolConfiguration.Value.SqlEndpoint.Split(';').First() },
                { "DATABASE", $"{owner}_1" }
            }
        };

        Datasets datasets = await this.datasetCommand.List(sharedDatasetRequest, cancellationToken);
        Dataset sharedDataset = datasets.Value.FirstOrDefault(d => d.Name == sharedDatasetName);

        try
        {
            sharedDataset ??= await this.datasetCommand.Create(sharedDatasetRequest, cancellationToken);
        }
        catch (ServiceException ex) when (ex.ServiceError.Category == ErrorCategory.ServiceError && ex.ServiceError.Code == ErrorCode.PowerBI_ReportDeleteFailed.Code)
        {
            IDatasetRequest deleteRequest = new DatasetRequest()
            {
                DatasetId = Guid.Parse(sharedDataset.Id),
                ProfileId = profile.Id,
                WorkspaceId = workspace.Id
            };
            await this.datasetCommand.Delete(deleteRequest, cancellationToken);

            throw;
        }
        Report report = await this.reportCommand.Bind(sharedDataset, datasetRequest, cancellationToken);
        */
    }
}
