// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

[JobCallback(Name = nameof(DataQualitySparkJobCallback))]
internal class DataQualitySparkJobCallback : StagedWorkerJobCallback<SparkJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    public DataQualitySparkJobCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    protected override string JobName => nameof(DataQualitySparkJobCallback);

    // Postpone for 30 seconds in tracking stage giving max of 45 minutes for job to complete before retrying.
    protected override int MaxPostponeCount => 90;

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }

    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new TriggerDataQualitySparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackDataQualitySparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
        };
    }

    protected override async Task TransitionToJobFailed()
    {
        await Task.CompletedTask;
        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
    }

    protected override async Task TransitionToJobSucceeded()
    {
        await Task.CompletedTask;
        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
    }

    private void ResetJobWorkingState()
    {
        this.Metadata.SparkJobBatchId = string.Empty;
        this.Metadata.IsCompleted = false;
    }
}
