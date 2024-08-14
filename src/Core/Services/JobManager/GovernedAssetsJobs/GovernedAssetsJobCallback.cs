// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.GovernedAssetsJobs;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(GovernedAssetsJobCallback))]
internal class GovernedAssetsJobCallback : StagedWorkerJobCallback<GovernedAssetsJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private readonly ISparkJobManager sparkJobManager;

    public GovernedAssetsJobCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();
    }

    protected override string JobName => nameof(GovernedAssetsJobCallback);

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }
    protected override bool IsJobReachMaxExecutionTime()
    {
        /*if (this.Metadata.CurrentScheduleStartTime != null)
        {
            return DateTime.UtcNow > this.Metadata.CurrentScheduleStartTime?.AddHours(1.5);
        }*/
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
            new ListAccountStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new ProcessComputingAssetsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
        };
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
        this.Metadata.ListAccountsStageProcessed = false;
        this.Metadata.GovernedAssetsJobAccounts = new List<GovernedAssetsJobAccount>();
        this.Metadata.SparkPoolId = string.Empty;
        this.Metadata.CurrentScheduleStartTime = null;
    }

    private async Task DeleteSparkPools()
    {
        await Task.CompletedTask;
        // Delete spark pools
        /*try
        {
            await this.sparkJobManager.DeleteSparkPool(this.Metadata.SparkPoolId, new CancellationToken());
        }
        catch (Exception e)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to delete spark pool. SparkPoolId: {this.Metadata.SparkPoolId}", e);
        }*/
    }
}
