// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(PBIRefreshCallback))]
internal class PBIRefreshCallback : StagedWorkerJobCallback<StartPBIRefreshMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <inheritdoc />
    protected override string JobName => nameof(PBIRefreshCallback);

    public PBIRefreshCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    /// <inheritdoc />
    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
        foreach (var stage in this.JobStages)
        {
            if (!stage.IsStageComplete())
            {
                return;
            }
        }
        result.Status = JobExecutionStatus.Completed;
        this.dataEstateHealthRequestLogger.LogInformation($"All stages are completed. Change job status to completed. {this.JobPartition} {this.JobId}");
    }

    /// <inheritdoc />
    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new StartPBIReportUpgradeStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new StartPBIRefreshStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new EndPBIRefreshStage(this.Scope, this.Metadata, this.JobCallbackUtils)
        };
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobFailed()
    {
        await Task.CompletedTask;

        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobSucceeded()
    {
        await Task.CompletedTask;

        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
    }

    private void ResetJobWorkingState()
    {
        this.Metadata.RefreshLookups.Clear();
    }
}
