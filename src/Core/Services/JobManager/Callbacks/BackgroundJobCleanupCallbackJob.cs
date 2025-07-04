﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(BackgroundJobCleanupCallbackJob))]
internal class BackgroundJobCleanupCallbackJob : StagedWorkerJobCallback<StagedWorkerJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <inheritdoc />
    protected override string JobName => nameof(BackgroundJobCleanupCallbackJob);

    public BackgroundJobCleanupCallbackJob(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    /// <inheritdoc />
    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override bool IsJobReachMaxExecutionTime()
    {
        return false;
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
            new ResetFaultedJobStage(this.Scope, this.JobManagement),
            new CleanupSparkPoolsJobStage(this.Scope, this.JobManagement),
        };
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobFailed()
    {
        await Task.CompletedTask;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobSucceeded()
    {
        await Task.CompletedTask;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
    }
}
