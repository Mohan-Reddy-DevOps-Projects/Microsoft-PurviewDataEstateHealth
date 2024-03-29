// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using System.Threading;
using System.Threading.Tasks;

[Component(typeof(IPartnerNotificationComponent), ServiceVersion.V1)]
internal sealed class PartnerNotificationComponent : BaseComponent<IPartnerNotificationContext>, IPartnerNotificationComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IHealthProfileCommand profileCommand;

    [Inject]
    private readonly HealthWorkspaceCommand workspaceCommand;

    [Inject]
    private readonly IDatabaseManagementService databaseManagementService;

    [Inject]
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

    [Inject]
    private readonly CapacityProvider capacityAssignment;

    [Inject]
    private readonly IHealthPBIReportComponent healthPBIReportComponent;

    [Inject]
    private readonly ISparkJobManager sparkJobManager;

    [Inject]
    private readonly IArtifactStoreAccountComponent artifactStoreAccountComponent;

    [Inject]
    private readonly IJobManager backgroundJobManager;

    [Inject]
    private readonly IAccountExposureControlConfigProvider exposureControl;

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
        await this.ProvisionSparkJobs(account);
        await this.ProvisionActionsCleanUpJob(account);
        await this.artifactStoreAccountComponent.CreateArtifactStoreResources(account, cancellationToken);
    }

    public async Task DeleteNotification(AccountServiceModel account, CancellationToken cancellationToken)
    {
        await this.DeprovisionSparkJobs(account);
        await this.DeprovisionActionCleanUpJob(account);
        await this.DeletePowerBIResources(account, cancellationToken);
        await this.databaseManagementService.Deprovision(account, cancellationToken);
        await this.sparkJobManager.DeleteSparkPool(account, cancellationToken);
    }

    private async Task CreatePowerBIResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        ProfileKey profileKey = new(this.Context.AccountId);
        IProfileModel profile = await this.profileCommand.Create(profileKey, cancellationToken);
        IWorkspaceContext context = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id
        };
        Group workspace = await this.workspaceCommand.Create(context, cancellationToken);

        await this.capacityAssignment.AssignWorkspace(profile.Id, workspace.Id, cancellationToken);
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, OwnerNames.Health, cancellationToken);
        if (powerBICredential == null)
        {
            // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
            powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, OwnerNames.Health);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
        }

        await this.healthPBIReportComponent.CreateDataGovernanceReport(account, profile.Id, workspace.Id, powerBICredential, cancellationToken);
    }

    private async Task DeletePowerBIResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        ProfileKey profileKey = new(this.Context.AccountId);
        IProfileModel profile = await this.profileCommand.Get(profileKey, cancellationToken);
        IWorkspaceContext context = new WorkspaceContext(this.Context)
        {
            ProfileId = profile.Id
        };
        await this.workspaceCommand.Delete(context, cancellationToken);
        await this.profileCommand.Delete(profileKey, cancellationToken);
    }

    private async Task ProvisionSparkJobs(AccountServiceModel account)
    {
        if (this.exposureControl.IsDataGovProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.ProvisionCatalogSparkJob(account);
        }

        if (this.exposureControl.IsDataQualityProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.ProvisionDataQualitySparkJob(account);
        }
    }

    private async Task ProvisionActionsCleanUpJob(AccountServiceModel account)
    {
        if (this.exposureControl.IsDGDataHealthEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.ProvisionActionsCleanupJob(account);
        }
    }
    private async Task DeprovisionSparkJobs(AccountServiceModel account)
    {
        if (this.exposureControl.IsDataGovProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.DeprovisionCatalogSparkJob(account);
        }

        if (this.exposureControl.IsDataQualityProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.DeprovisionDataQualitySparkJob(account);
        }
    }

    private async Task DeprovisionActionCleanUpJob(AccountServiceModel account)
    {
        if (this.exposureControl.IsDGDataHealthEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.DeprovisionActionsCleanupJob(account);
        }
    }
}
