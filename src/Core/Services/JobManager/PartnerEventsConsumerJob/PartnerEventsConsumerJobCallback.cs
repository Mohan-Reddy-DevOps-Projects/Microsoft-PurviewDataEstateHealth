// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

[JobCallback(Name = nameof(PartnerEventsConsumerJobCallback))]
internal class PartnerEventsConsumerJobCallback : StagedWorkerJobCallback<PartnerEventsConsumerJobMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <inheritdoc />
    protected override string JobName => nameof(PartnerEventsConsumerJobCallback);

    public PartnerEventsConsumerJobCallback(IServiceScope scope)
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
    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    protected override void OnJobConfigure()
    {
        this.JobStages = new List<IJobCallbackStage>
        {
            new DataCatalogEventsProcessingStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataAccessEventsProcessingStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new DataQualityEventsProcessingStage(this.Scope, this.Metadata, this.JobCallbackUtils),
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
        this.Metadata.DataAccessEventsProcessed = false;
        this.Metadata.DataCatalogEventsProcessed = false;
        this.Metadata.DataQualityEventsProcessed = false;
    }
}
