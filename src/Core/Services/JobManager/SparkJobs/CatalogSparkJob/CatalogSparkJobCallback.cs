// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(CatalogSparkJobCallback))]
internal class CatalogSparkJobCallback : StagedWorkerJobCallback<DataPlaneSparkJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    public CatalogSparkJobCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    protected override string JobName => nameof(CatalogSparkJobCallback);

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }
    protected override bool IsJobReachMaxExecutionTime()
    {
        if (this.Metadata.CurrentScheduleStartTime != null)
        {
            return DateTime.UtcNow > this.Metadata.CurrentScheduleStartTime?.AddHours(1.5);
        }
        return false;
    }

    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new TriggerCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TriggerDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
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
        this.Metadata.CatalogSparkJobBatchId = string.Empty;
        this.Metadata.DimensionSparkJobBatchId = string.Empty;
        this.Metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.CurrentScheduleStartTime = null;
    }
}
