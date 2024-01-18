// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class EndPBIRefreshStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils;
    private readonly IServiceScope scope;
    private readonly StartPBIRefreshMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly CapacityProvider capacityAssignment;
    private readonly IRefreshComponent refreshComponent;
    private readonly IHealthPBIReportComponent healthPBIReportComponent;
    private readonly DatasetProvider datasetProvider;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly IAccountExposureControlConfigProvider exposureControl;

    private static readonly TimeSpan postPoneTime = TimeSpan.FromMinutes(10);

    public EndPBIRefreshStage(
        IServiceScope scope,
        StartPBIRefreshMetadata metadata,
        JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils)
    {
        this.scope = scope;
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        this.capacityAssignment = scope.ServiceProvider.GetRequiredService<CapacityProvider>();
        this.refreshComponent = scope.ServiceProvider.GetRequiredService<IRefreshComponent>();
        this.healthPBIReportComponent = scope.ServiceProvider.GetRequiredService<IHealthPBIReportComponent>();
        this.datasetProvider = scope.ServiceProvider.GetRequiredService<DatasetProvider>();
        this.powerBICredentialComponent = scope.ServiceProvider.GetRequiredService<IPowerBICredentialComponent>();
        this.exposureControl = scope.ServiceProvider.GetRequiredService<IAccountExposureControlConfigProvider>();
    }

    public string StageName => nameof(EndPBIRefreshStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        try
        {
            IList<RefreshDetailsModel> refreshDetails = await this.refreshComponent.GetRefreshStatus(this.metadata.RefreshLookups, CancellationToken.None);
            HashSet<Guid> datasetsPendingRefresh = this.ProcessRefreshDetails(refreshDetails);

            if (datasetsPendingRefresh.Count == 0)
            {
                PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(Guid.Parse(this.metadata.Account.Id), OwnerNames.Health, CancellationToken.None);
                await this.ProcessSuccessfulRefreshDetails(refreshDetails, powerBICredential, CancellationToken.None);
                JobExecutionStatus jobExecutionStatus = refreshDetails.All(ReachedSuccessfulStatus) ? JobExecutionStatus.Succeeded : JobExecutionStatus.Failed;

                return this.UpdateJobStatus(jobExecutionStatus);
            }
            this.metadata.RefreshLookups = this.metadata.RefreshLookups.Where(x => datasetsPendingRefresh.Contains(x.DatasetId)).ToArray();

            return this.UpdateJobStatus(JobExecutionStatus.Postponed, DateTime.UtcNow.Add(postPoneTime));
        }
        catch (Exception exception)
        {
            this.logger.LogError($"Error starting PBI refresh from {this.StageName}", exception);
            jobStageStatus = JobExecutionStatus.Failed;
            jobStatusMessage = FormattableString.Invariant($"Errored tracking PBI refresh from {this.StageName}.");
            this.logger.LogTrace(jobStatusMessage);

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.RefreshLookups.Count == 0;
    }

    public bool IsStagePreconditionMet()
    {
        return this.metadata.RefreshLookups.Count != 0;
    }


    /// <summary>
    /// Check if endTime is present to ensure refresh completed.
    /// </summary>
    /// <param name="refresh"></param>
    /// <returns></returns>
    private static bool ReachedTerminalStatus(RefreshDetailsModel refresh)
    {
        return refresh.EndTime != null;
    }

    /// <summary>
    /// Check if the refresh has completed successfully.
    /// </summary>
    /// <param name="refresh"></param>
    /// <returns></returns>
    private static bool ReachedSuccessfulStatus(RefreshDetailsModel refresh)
    {
        return refresh.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Update the job status.
    /// </summary>
    /// <param name="jobStageStatus"></param>
    /// <param name="nextExecutionTime"></param>
    /// <returns></returns>
    private JobExecutionResult UpdateJobStatus(JobExecutionStatus jobStageStatus, DateTime? nextExecutionTime = null)
    {
        string jobStatusMessage = $"{jobStageStatus} stage {this.StageName}";
        this.logger.LogTrace(jobStatusMessage);

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, nextExecutionTime.HasValue ? nextExecutionTime : null);
    }

    /// <summary>
    /// Process all refreshes. When the refresh is in progress, return the datasetId to continue to monitor in the next iteration.
    /// </summary>
    /// <param name="refreshDetails"></param>
    /// <returns></returns>
    private HashSet<Guid> ProcessRefreshDetails(IEnumerable<RefreshDetailsModel> refreshDetails)
    {
        var datasetsPendingRefresh = new HashSet<Guid>();
        foreach (var refreshDetail in refreshDetails)
        {
            var datasetId = this.ProcessRefresh(refreshDetail);
            if (datasetId != Guid.Empty)
            {
                datasetsPendingRefresh.Add(datasetId);
            }
        }
        return datasetsPendingRefresh;
    }

    /// <summary>
    /// For all successful refreshes, execute operations to complete pbi refresh.
    /// </summary>
    /// <param name="refreshDetails"></param>
    /// <param name="credential"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessSuccessfulRefreshDetails(IEnumerable<RefreshDetailsModel> refreshDetails, PowerBICredential credential, CancellationToken cancellationToken)
    {
        IEnumerable<RefreshDetailsModel> successfulRefreshes = refreshDetails.Where(refreshDetail => ReachedSuccessfulStatus(refreshDetail) && this.exposureControl.IsDataGovProvisioningEnabled(this.metadata.Account.Id, this.metadata.Account.SubscriptionId, this.metadata.Account.TenantId));
        foreach (RefreshDetailsModel refreshDetail in successfulRefreshes)
        {
            await this.CreateNewReport(refreshDetail, credential, cancellationToken);
        }
    }

    /// <summary>
    /// Process a refresh. When the refresh is in progress, return the datasetId to continue to monitor in the next iteration.
    /// When the refresh is failed, delete the dataset and refresh the prior version(s) of the dataset.
    /// </summary>
    /// <param name="refreshDetail"></param>
    /// <returns></returns>
    private Guid ProcessRefresh(RefreshDetailsModel refreshDetail)
    {
        this.logger.LogWarning($"{this.StageName}|DatasetId: {refreshDetail.DatasetId}; Status: {refreshDetail.Status}; Type: {refreshDetail.Type}; StartTime: {refreshDetail.StartTime}; EndTime: {refreshDetail.EndTime}");
        if (!ReachedTerminalStatus(refreshDetail))
        {
            return refreshDetail.DatasetId;
        }
        TimeSpan? elapsedTime = refreshDetail.EndTime - refreshDetail.StartTime;
        this.logger.LogInformation($"{this.StageName}|Completed refresh in {elapsedTime.Value.TotalMinutes} minutes. Details={refreshDetail}");


        return Guid.Empty;
    }

    /// <summary>
    /// Create a new report for the dataset and delete the existing datasets.
    /// </summary>
    /// <param name="refreshDetail"></param>
    /// <param name="powerBICredential"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task CreateNewReport(RefreshDetailsModel refreshDetail, PowerBICredential powerBICredential, CancellationToken cancellationToken)
    {
        IDatasetRequest reportRequest = this.healthPBIReportComponent.GetReportRequest(refreshDetail.ProfileId, refreshDetail.WorkspaceId, powerBICredential, HealthReportNames.DataGovernance);
        if (this.metadata.DatasetUpgrades.TryGetValue(refreshDetail.DatasetId, out List<PowerBI.Api.Models.Dataset> existingDatasets))
        {
            var sharedDataset = await this.datasetProvider.Get(refreshDetail.ProfileId, refreshDetail.WorkspaceId, refreshDetail.DatasetId, cancellationToken);
            var report = await this.healthPBIReportComponent.CreateReport(sharedDataset, reportRequest, CancellationToken.None, true);
            IEnumerable<Task<DeletionResult>> tasks = existingDatasets.Select(async x => await this.datasetProvider.Delete(refreshDetail.ProfileId, refreshDetail.WorkspaceId, Guid.Parse(x.Id), cancellationToken));
            DeletionResult[] deletedDatasets = await Task.WhenAll(tasks);
        }
    }
}
