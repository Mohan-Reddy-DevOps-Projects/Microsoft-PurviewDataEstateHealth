// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
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
    private readonly IJobManager backgroundJobManager;

    [Inject]
    private readonly IAccountExposureControlConfigProvider exposureControl;

    [Inject]
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

#pragma warning restore 649
    public PartnerNotificationComponent(IPartnerNotificationContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateNotification(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to create/update DEH resources, account name: {account.Name}"))
        {
            List<Task> tasks =
            [
                this.ProvisioningSparkRelatedTask(account, cancellationToken),
                this.ProvisioningPowerBIRelatedTask(account, cancellationToken),
                this.ProvisionActionsCleanUpJob(account),
            ];
            await Task.WhenAll(tasks);
        }
    }

    private async Task ProvisioningSparkRelatedTask(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to create/update spark related resources, account name: {account.Name}"))
        {
            //await this.sparkJobManager.CreateOrUpdateSparkPool(account, cancellationToken);
            await this.ProvisionSparkJobs(account);
        }
    }

    private async Task ProvisioningPowerBIRelatedTask(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to create/update powerBI related resources, account name: {account.Name}"))
        {
            await this.databaseManagementService.Initialize(account, cancellationToken);

            List<Task> tasks =
            [
                this.CreatePowerBIResources(account, cancellationToken),
                this.databaseManagementService.RunSetupSQL(cancellationToken)
            ];
            await Task.WhenAll(tasks);
        }
    }

    public async Task DeleteNotification(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to delete DEH resources, account name: {account.Name}"))
        {
            List<Task> tasks =
            [
                this.DeprovisioningSparkRelatedTask(account, cancellationToken),
                this.DeprovisioningPowerBIRelatedTask(account, cancellationToken),
                this.DeprovisionActionCleanUpJob(account),
            ];
            await Task.WhenAll(tasks);
        }
    }

    private async Task DeprovisioningSparkRelatedTask(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to delete spark related resources, account name: {account.Name}"))
        {
            await this.DeprovisionSparkJobs(account);
            await this.sparkJobManager.DeleteSparkPoolRecord(account, cancellationToken);
        }
    }

    private async Task DeprovisioningPowerBIRelatedTask(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to delete powerBI related resources, account name: {account.Name}"))
        {
            await this.DeletePowerBIResources(account, cancellationToken);
            await this.databaseManagementService.Deprovision(account, cancellationToken);
        }
    }

    private async Task CreatePowerBIResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to create/update PowerBI resources"))
        {
            try
            {
                ProfileKey profileKey = new(this.Context.AccountId);
                IProfileModel profile = await this.profileCommand.Create(profileKey, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Profile created successfully");
                IWorkspaceContext context = new WorkspaceContext(this.Context)
                {
                    ProfileId = profile.Id
                };
                Group workspace = await this.workspaceCommand.Create(context, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Workspace created successfully");

                await this.capacityAssignment.AssignWorkspace(profile.Id, workspace.Id, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Workspace assigned successfully");
                PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, OwnerNames.Health, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("PowerBI credential retrieved successfully");
                if (powerBICredential == null)
                {
                    // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
                    powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, OwnerNames.Health);
                    await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
                    this.dataEstateHealthRequestLogger.LogInformation("PowerBI credential created successfully");
                }

                await this.healthPBIReportComponent.CreateDataGovernanceReport(account, profile.Id, workspace.Id, powerBICredential, cancellationToken);
                await this.healthPBIReportComponent.CreateDataQualityReport(account, profile.Id, workspace.Id, powerBICredential, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("PowerBI report created successfully");
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to create PowerBI resources", e);
                throw;
            }
        }
    }

    private async Task DeletePowerBIResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to delete PowerBI resources"))
        {
            ProfileKey profileKey = new(this.Context.AccountId);
            IProfileModel profile;
            try
            {
                profile = await this.profileCommand.Get(profileKey, cancellationToken);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to get profile", e);
                return;
            }
            IWorkspaceContext context = new WorkspaceContext(this.Context)
            {
                ProfileId = profile.Id
            };
            try
            {
                await this.workspaceCommand.Delete(context, cancellationToken);
                await this.profileCommand.Delete(profileKey, cancellationToken);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to delete PowerBI resources", e);
                throw;
            }
        }
    }

    private async Task ProvisionSparkJobs(AccountServiceModel account)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to provision spark jobs, account name: {account.Name}"))
        {
            try
            {
                await this.backgroundJobManager.ProvisionCatalogSparkJob(account);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError($"provisioning catalog spark job with failure", e);
                throw;
            }
        }
    }

    private async Task ProvisionActionsCleanUpJob(AccountServiceModel account)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"start to provision action clean up job, account name: {account.Name}"))
        {
            try
            {
                await this.backgroundJobManager.ProvisionActionsCleanupJob(account);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError($"provisioning action clean up job with failure", e);
                throw;
            }
        }
    }
    private async Task DeprovisionSparkJobs(AccountServiceModel account)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to delete spark jobs"))
        {
            try
            {
                await this.backgroundJobManager.DeprovisionCatalogSparkJob(account);
                try
                {
                    if (this.exposureControl.IsDataQualityProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
                    {
                        await this.backgroundJobManager.DeprovisionDataQualitySparkJob(account);
                    }
                }
                catch (Exception ex)
                {
                    this.dataEstateHealthRequestLogger.LogError($"deprovisioning DQ spark job with failure", ex);
                }
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError($"deprovisioning spark job with failure", e);
                throw;
            }

        }
    }

    private async Task DeprovisionActionCleanUpJob(AccountServiceModel account)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to delete actions cleanup jobs"))
        {
            try
            {
                await this.backgroundJobManager.DeprovisionActionsCleanupJob(account);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError($"deprovisioning action clean up job with failure", e);
                throw;
            }
        }
    }
}
