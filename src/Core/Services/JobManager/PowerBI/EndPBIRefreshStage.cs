// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Reporting;
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

    private static readonly TimeSpan postPoneTime = TimeSpan.FromMinutes(10);

    public EndPBIRefreshStage(
        IServiceScope scope,
        StartPBIRefreshMetadata metadata,
        JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils)
    {
        this.scope = scope;
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.capacityAssignment = scope.ServiceProvider.GetService<CapacityProvider>();
        this.refreshComponent = scope.ServiceProvider.GetService<IRefreshComponent>();
    }

    public string StageName => nameof(EndPBIRefreshStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        try
        {
            IList<RefreshDetailsModel> refreshDetails = await this.refreshComponent.GetRefreshStatus(this.metadata.RefreshLookups, CancellationToken.None);
            HashSet<Guid> datasetsPendingRefresh = new();

            foreach (RefreshDetailsModel refreshDetail in refreshDetails)
            {
                Guid datasetId = this.ProcessRefresh(refreshDetail);
                if (datasetId != Guid.Empty)
                {
                    datasetsPendingRefresh.Add(datasetId);
                }
            }

            if (datasetsPendingRefresh.Count == 0)
            {
                this.metadata.RefreshLookups.Clear();
                if (refreshDetails.All(ReachedSuccessfulStatus))
                {
                    jobStageStatus = JobExecutionStatus.Completed;
                    jobStatusMessage = $"Completed stage {this.StageName}";
                    this.logger.LogTrace(jobStatusMessage);

                    return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage);
                }
                else
                {
                    jobStageStatus = JobExecutionStatus.Faulted;
                    jobStatusMessage = $"Faulted stage {this.StageName}";
                    this.logger.LogTrace(jobStatusMessage);

                    return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage);
                }
            }
            this.metadata.RefreshLookups = this.metadata.RefreshLookups.Where(x => datasetsPendingRefresh.Contains(x.DatasetId)).ToArray();

            jobStageStatus = JobExecutionStatus.Postponed;
            jobStatusMessage = $"Postponed stage {this.StageName}";
            this.logger.LogTrace(jobStatusMessage);

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(postPoneTime));
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
}
