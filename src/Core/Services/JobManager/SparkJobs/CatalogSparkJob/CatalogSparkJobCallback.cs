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
internal class CatalogSparkJobCallback(IServiceScope scope) : StagedWorkerJobCallback<DataPlaneSparkJobMetadata>(scope)
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly ISparkJobManager sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();

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
        this.JobStages =
        [
            new TriggerCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TriggerDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TriggerFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            /*new TriggerComputeGovernedAssetsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackComputeGovernedAssetsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),*/
        ];
    }

    protected override async Task TransitionToJobFailed()
    {
        using (this.DataEstateHealthRequestLogger.LogElapsed($"Transition to job failed, name: {this.JobName}"))
        {
            await this.DeleteSparkPools();
            this.ResetJobWorkingState();
        }
    }

    protected override async Task TransitionToJobSucceeded()
    {
        using (this.DataEstateHealthRequestLogger.LogElapsed($"Transition to job succeeded, name: {this.JobName}"))
        {
            await this.DeleteSparkPools();
            this.ResetJobWorkingState();
        }
    }

    private void ResetJobWorkingState()
    {
        this.Metadata.CatalogSparkJobBatchId = string.Empty;
        this.Metadata.DimensionSparkJobBatchId = string.Empty;
        this.Metadata.SparkPoolId = string.Empty;
        this.Metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.CurrentScheduleStartTime = null;
    }

    private async Task DeleteSparkPools()
    {
        // Delete spark pools
        try
        {
            await this.sparkJobManager.DeleteSparkPool(this.Metadata.SparkPoolId, new CancellationToken());
        }
        catch (Exception e)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to delete spark pool. SparkPoolId: {this.Metadata.SparkPoolId}", e);
        }
    }
}
