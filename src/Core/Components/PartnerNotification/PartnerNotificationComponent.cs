// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
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
    private readonly IDatabaseManagementService databaseManagementService;

    [Inject]
    private readonly IReportCommand reportCommand;

    [Inject]
    private readonly IDatasetCommand datasetCommand;

    [Inject]
    private readonly IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration;

    [Inject]
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

    [Inject]
    private readonly ICapacityAssignment capacityAssignment;

    [Inject]
    private readonly ISparkJobManager sparkJobManager;

    [Inject]
    private readonly IArtifactStoreAccountComponent artifactStoreAccountComponent;
    
    [Inject]
    private readonly IJobManager backgroundJobManager;

#pragma warning restore 649

    public PartnerNotificationComponent(IPartnerNotificationContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateNotification(AccountServiceModel account, CancellationToken cancellationToken)
    {
        await this.sparkJobManager.CreateOrUpdateSparkPool(account, cancellationToken);
        await this.databaseManagementService.Initialize(account, cancellationToken);
        await this.CreatePowerBIResources(account, cancellationToken);
        await this.artifactStoreAccountComponent.CreateArtifactStoreResources(account, cancellationToken);
        await this.ProvisionSparkJobs(account);
    }

    private async Task CreatePowerBIResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        IProfileModel profile = await this.profileCommand.Create(this.Context.AccountId, cancellationToken);
        IWorkspaceContext context = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id
        };
        Group workspace = await this.workspaceCommand.Create(context, cancellationToken);

        await this.capacityAssignment.AssignWorkspace(profile.Id, workspace.Id, cancellationToken);
        string owner = "health";
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, owner, cancellationToken);
        if (powerBICredential == null)
        {
            // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
            powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, owner);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
        }

        string reportName = HealthReportNames.DataGovernance;
        IDatasetRequest datasetRequest = new DatasetRequest()
        {
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            DatasetContainer = "powerbi",
            DatasetFileName = $"{reportName}.pbix",
            DatasetName = reportName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential
        };

        string sharedDatasetName = SystemDatasets.Get()[HealthDataset.Dataset.DataGovernance.ToString()].Name;
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
                { SQLConstants.Server, this.serverlessPoolConfiguration.Value.SqlEndpoint.Split(';').First() },
                { SQLConstants.Database, $"{owner}_1" },
                { SQLConstants.DatabaseSchema, account.Id },
                { "TENANT_ID", account.TenantId }
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
        IReportRequest reportRequest = new ReportRequest()
        {
            ProfileId = datasetRequest.ProfileId,
            WorkspaceId = datasetRequest.WorkspaceId,
        };
        Reports existingReports = await this.reportCommand.List(reportRequest, cancellationToken);
        if (!existingReports.Value.Any(r => r.Name == datasetRequest.DatasetName))
        {
            Report report = await this.reportCommand.Bind(sharedDataset, datasetRequest, cancellationToken);
        }
    }

    private async Task ProvisionSparkJobs(AccountServiceModel account)
    {
        await backgroundJobManager.ProvisionCatalogSparkJob(account);
    }
}
