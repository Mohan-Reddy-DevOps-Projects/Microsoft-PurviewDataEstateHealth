// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class StartPBIReportUpgradeStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils;
    private readonly IServiceScope scope;
    private readonly StartPBIRefreshMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IAccountExposureControlConfigProvider exposureControl;
    private readonly IHealthProfileCommand profileCommand;
    private readonly HealthWorkspaceCommand workspaceCommand;
    private readonly DatasetProvider datasetCommand;
    private readonly IHealthPBIReportComponent healthPBIReportComponent;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly CapacityProvider capacityAssignment;

    public StartPBIReportUpgradeStage(
    IServiceScope scope,
    StartPBIRefreshMetadata metadata,
    JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils)
    {
        this.scope = scope;
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
        this.profileCommand = scope.ServiceProvider.GetRequiredService<IHealthProfileCommand>();
        this.workspaceCommand = scope.ServiceProvider.GetRequiredService<HealthWorkspaceCommand>();
        this.datasetCommand = scope.ServiceProvider.GetRequiredService<DatasetProvider>();
        this.healthPBIReportComponent = scope.ServiceProvider.GetRequiredService<IHealthPBIReportComponent>();
        this.powerBICredentialComponent = scope.ServiceProvider.GetRequiredService<IPowerBICredentialComponent>();
        this.capacityAssignment = scope.ServiceProvider.GetRequiredService<CapacityProvider>();
    }

    public string StageName => nameof(StartPBIReportUpgradeStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        try
        {

            Guid accountId = Guid.Parse(this.metadata.Account.Id);
            ProfileKey profileKey = new(accountId);
            IProfileModel profileModel = await this.profileCommand.Get(profileKey, CancellationToken.None);
            IWorkspaceContext workspaceContext = new WorkspaceContext()
            {
                AccountId = accountId,
                ProfileId = profileModel.Id,
            };
            Group workspace = await this.workspaceCommand.Get(workspaceContext, CancellationToken.None);

            // make sure this list is updated so refresh happens this.metadata.DatasetUpgrades
            await this.CreatePowerBIResources(this.metadata, this.metadata.Account, CancellationToken.None);
            //HealthDatasetUpgrade insightsDatasetUpgrade = new(this.logger, this.datasetCommand, this.healthPBIReportComponent, this.powerBICredentialComponent);
            //Dictionary<Guid, List<Dataset>> datasetUpgrades = await insightsDatasetUpgrade.UpgradeDatasets(this.metadata.Account, profileModel.Id, workspace.Id, true, CancellationToken.None);
            ////datasetUpgrades = await insightsDatasetUpgrade.UpgradeDatasets(this.metadata.Account, profileModel.Id, workspace.Id, true, SystemDatasets.Get()[HealthDataset.Dataset.DataGovernance.ToString()].Name, CancellationToken.None);
            //this.metadata.DatasetUpgrades = datasetUpgrades;
            this.metadata.ProfileId = profileModel.Id;
            this.metadata.WorkspaceId = workspace.Id;

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = this.GenerateStatusMessage(jobStageStatus);
            this.logger.LogInformation(jobStatusMessage);
        }
        catch (Exception exception)
        {
            this.metadata.DatasetUpgrades = [];
            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"{this.StageName}|Failed to upgrade PBI reports account: {this.metadata.Account.Id} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        var jobStatusSuccessMessage = $"{this.StageName}|Succeed to upgrade PBI reports account: {this.metadata.Account.Id}";
        this.logger.LogInformation(jobStatusSuccessMessage);
        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }


    private async Task CreatePowerBIResources(StartPBIRefreshMetadata metadata, AccountServiceModel account,
            CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("start to create/update PowerBI resources"))
        {
            try
            {
                ProfileKey profileKey = new(Guid.Parse(account.Id));
                IProfileModel profile = await this.profileCommand.Create(profileKey, cancellationToken);
                this.logger.LogInformation("Profile created successfully");
                IWorkspaceContext context = new WorkspaceContext()// this.Context)
                {
                    ProfileId = profile.Id,
                    AccountId = Guid.Parse(this.metadata.Account.Id),
                    TenantId = Guid.Parse(this.metadata.Account.TenantId)
                };
                Group workspace = await this.workspaceCommand.Create(context, cancellationToken);
                this.logger.LogInformation("Workspace created successfully");

                await this.capacityAssignment.AssignWorkspace(profile.Id, workspace.Id, cancellationToken);
                this.logger.LogInformation("Workspace assigned successfully");
                PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(context.AccountId, OwnerNames.Health, cancellationToken);
                this.logger.LogInformation("PowerBI credential retrieved successfully");
                if (powerBICredential == null)
                {
                    // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
                    powerBICredential = this.powerBICredentialComponent.CreateCredential(context.AccountId, OwnerNames.Health);
                    await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
                    this.logger.LogInformation("PowerBI credential created successfully");
                }

                await this.RetryAsync(() => this.healthPBIReportComponent.CreateDataGovernanceReport(account, profile.Id, workspace.Id, powerBICredential, cancellationToken, metadata: metadata), nameof(this.healthPBIReportComponent.CreateDataGovernanceReport));

                await this.RetryAsync(() => this.healthPBIReportComponent.CreateDataQualityReport(account, profile.Id, workspace.Id, powerBICredential, cancellationToken, metadata: metadata), nameof(this.healthPBIReportComponent.CreateDataQualityReport));

                this.logger.LogInformation("PowerBI report created successfully");
            }
            catch (Exception e)
            {
                this.logger.LogError("Failed to create PowerBI resources", e);
                throw;
            }
        }
    }

    public async Task RetryAsync(Func<Task> action, string functionName = "", int maxRetries = 5, int delayMilliseconds = 1000)
    {
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                await action().ConfigureAwait(false);
                this.logger.LogInformation($"{functionName} completed successfully in {retryCount} attempt");
                return; // Exit if successful
            }
            catch (Exception ex)
            {
                retryCount++;
                this.logger.LogInformation($"{functionName} attempt {retryCount} failed: {ex.Message}");

                if (retryCount == maxRetries)
                {
                    this.logger.LogInformation($"{functionName} All retry attempts failed.");
                    throw; // Rethrow the exception if all retries fail
                }

                // Optional: Add a delay before retrying
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.DatasetUpgrades != null;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }

    private string GenerateStatusMessage(JobExecutionStatus status)
    {
        string accountId = this.metadata.Account.Id;
        string detail = status switch
        {
            JobExecutionStatus.Succeeded => $"{this.StageName}|{status} for account: {accountId}.",
            JobExecutionStatus.Completed => $"{this.StageName}|failed for account: {accountId}",
            JobExecutionStatus.Postponed => $"{this.StageName}|{status} for account: {accountId}.",
            _ => $"Unknown status for {this.StageName} for account: {accountId}."
        };
        return detail;
    }
}
