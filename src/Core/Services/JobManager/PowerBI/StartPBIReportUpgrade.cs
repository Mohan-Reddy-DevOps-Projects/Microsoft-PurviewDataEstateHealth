// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class StartPBIReportUpgradeStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils;
    private readonly IServiceScope scope;
    private readonly StartPBIRefreshMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IAccountExposureControlConfigProvider exposureControl;
    private readonly HealthProfileCommand profileCommand;
    private readonly HealthWorkspaceCommand workspaceCommand;
    private readonly DatasetProvider datasetCommand;
    private readonly IHealthPBIReportComponent healthPBIReportComponent;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

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
        this.profileCommand = scope.ServiceProvider.GetRequiredService<HealthProfileCommand>();
        this.workspaceCommand = scope.ServiceProvider.GetRequiredService<HealthWorkspaceCommand>();
        this.datasetCommand = scope.ServiceProvider.GetRequiredService<DatasetProvider>();
        this.healthPBIReportComponent = scope.ServiceProvider.GetRequiredService<IHealthPBIReportComponent>();
        this.powerBICredentialComponent = scope.ServiceProvider.GetRequiredService<IPowerBICredentialComponent>();
    }

    public string StageName => nameof(StartPBIReportUpgradeStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        try
        {
            Guid accountId = Guid.Parse(this.metadata.Account.Id);
            IProfileModel profileModel = await this.profileCommand.Get(accountId, CancellationToken.None);
            IWorkspaceContext workspaceContext = new WorkspaceContext()
            {
                AccountId = accountId,
                ProfileId = profileModel.Id,
            };
            Group workspace = await this.workspaceCommand.Get(workspaceContext, CancellationToken.None);
            HealthDatasetUpgrade insightsDatasetUpgrade = new(this.logger, this.datasetCommand, this.healthPBIReportComponent, this.powerBICredentialComponent);
            Dictionary<Guid, List<Dataset>> datasetUpgrades = new();

            if (this.exposureControl.IsDataGovHealthPBIUpgradeEnabled(this.metadata.Account.Id, this.metadata.Account.SubscriptionId, this.metadata.Account.TenantId))
            {
                datasetUpgrades = await insightsDatasetUpgrade.UpgradeDatasets(this.metadata.Account, profileModel.Id, workspace.Id, true, CancellationToken.None);
            }
            this.metadata.DatasetUpgrades = datasetUpgrades;
            this.metadata.ProfileId = profileModel.Id;
            this.metadata.WorkspaceId = workspace.Id;

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = this.GenerateStatusMessage(jobStageStatus);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"{this.StageName}|Failed to upgrade PBI reports account: {this.metadata.Account.Id} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
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
