// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(FabricSparkJobCallback))]
internal class FabricSparkJobCallback : StagedWorkerJobCallback<SparkJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private readonly ISparkJobManager sparkJobManager;

    public FabricSparkJobCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();
    }

    protected override string JobName => nameof(FabricSparkJobCallback);

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override bool IsJobReachMaxExecutionTime()
    {
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
            //Not used anymore keeping it just in case if we need to reverse it
            //new TriggerFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            //new TrackFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
        };
    }

    protected override async Task TransitionToJobFailed()
    {
        await this.DeleteSparkPools();
        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
    }

    protected override async Task TransitionToJobSucceeded()
    {
        await this.DeleteSparkPools();
        this.ResetJobWorkingState();
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
    }

    private void ResetJobWorkingState()
    {
        this.Metadata.SparkJobBatchId = string.Empty;
        this.Metadata.SparkPoolId = string.Empty;
        this.Metadata.IsCompleted = false;
    }

    private async Task DeleteSparkPools()
    {
        // Delete spark pools
        await this.sparkJobManager.DeleteSparkPool(this.Metadata.SparkPoolId, new CancellationToken());
    }
}
