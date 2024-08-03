// -----------------------------------------------------------------------
// <copyright file="MetersToBillingJobCallback.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// Meterting to Biolling Job Bacll Back
/// </summary>
[JobCallback(Name = nameof(MetersToBillingJobCallback))]
internal class MetersToBillingJobCallback : StagedWorkerJobCallback<MetersToBillingJobMetadata>
{

    public MetersToBillingJobCallback(IServiceScope scope)
        : base(scope)
    {
    }

    /// <inheritdoc />
    protected override string JobName => nameof(MetersToBillingJobCallback);

    /// <inheritdoc />
    protected override int MaxRetryCount => 1000;

    /// <inheritdoc />
    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new MetersToBillingJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
        };
    }

    /// <inheritdoc />
    protected override Task<bool> IsJobPreconditionMet()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobSucceeded()
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobFailed()
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        return Task.CompletedTask;
    }
}
