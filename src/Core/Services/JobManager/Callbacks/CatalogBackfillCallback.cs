namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Stages.Backfill;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(CatalogBackfillCallback))]
internal class CatalogBackfillCallback : StagedWorkerJobCallback<StartCatalogBackfillMetadata>
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private readonly IMetadataAccessorService _metadataAccessorService;

    protected override string JobName => nameof(CatalogBackfillCallback);

    public CatalogBackfillCallback(IServiceScope scope)
        : base(scope)
    {
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>() ?? throw new InvalidOperationException($"{nameof(IDataEstateHealthRequestLogger)} logger not found");
        this._metadataAccessorService = scope.ServiceProvider.GetService<IMetadataAccessorService>() ?? throw new InvalidOperationException($"{nameof(IMetadataAccessorService)} not found");
    }

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

        result.NextExecutionTime = null;

        try
        {
            var jobManager = this.Scope.ServiceProvider.GetService<IJobManager>();
            if (jobManager != null)
            {
                await jobManager.DeleteJobAsync(this.JobPartition, this.JobId);
                this.dataEstateHealthRequestLogger.LogInformation($"Job {this.JobId} has been deleted after completion.");
            }
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to delete job {this.JobId} after completion: {ex.Message}", ex);
        }
    }

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
            new RunCatalogBackfillStage(this.Scope, this.Metadata, this.JobCallbackUtils, this._metadataAccessorService)
        };
    }

    protected override async Task TransitionToJobFailed()
    {
        await Task.CompletedTask;

        this.Metadata.BackfillStatus = CatalogBackfillStatus.Failed;
        this.Metadata.EndTime = DateTime.UtcNow;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
    }

    protected override async Task TransitionToJobSucceeded()
    {
        await Task.CompletedTask;

        this.Metadata.BackfillStatus = CatalogBackfillStatus.Completed;
        this.Metadata.EndTime = DateTime.UtcNow;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
    }
}