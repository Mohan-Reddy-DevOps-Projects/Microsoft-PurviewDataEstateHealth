namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.SelfServeAnalyticsSparkJob;

using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Threading.Tasks;

[JobCallback(Name = nameof(AnalyticsSparkJobCallback))]
internal class AnalyticsSparkJobCallback(IServiceScope scope) : StagedWorkerJobCallback<DataPlaneSparkJobMetadata>(scope)
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly ISparkJobManager sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();
    private readonly IServiceScope serviceScope = scope;
    private readonly IMetadataAccessorService _metadataAccessorService = scope.ServiceProvider.GetService<IMetadataAccessorService>();
 
    protected override string JobName => nameof(AnalyticsSparkJobCallback);

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }
    protected override bool IsJobReachMaxExecutionTime()
    {
        if (this.Metadata.CurrentScheduleStartTime != null)
        {
            //Increasing the delete spark scenario to handle quota issue.
            return DateTime.UtcNow > this.Metadata.CurrentScheduleStartTime?.AddHours(6);
        }
        return false;
    }

    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    protected override void OnJobConfigure()
    {
        //JobSubmissionEvaluator jobSubmissionEvaluator = new JobSubmissionEvaluator(this.serviceScope);

        try
        {
            this.dataEstateHealthRequestLogger.LogInformation($"AnalyticsSparkJobCallback OnJobConfigure: Processing account ID: {this.Metadata.RequestContext.AccountId}");
            var metadataClient = this._metadataAccessorService.GetMetadataServiceClient();
            var accountModel = metadataClient.Accounts.GetAsync(this.Metadata.RequestContext.AccountId.ToString(), "ByAccountId");
            this.dataEstateHealthRequestLogger.LogInformation($"AnalyticsSparkJobCallback: Retrieved account information for {accountModel.Result.Name} (ID: {accountModel.Id}, Tenant: {accountModel.Result.TenantId})");
            this.Metadata.AccountServiceModel = accountModel.Result;
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"AnalyticsSparkJobCallback OnJobConfigure: Error Processing: {ex}");
        }

        this.JobStages = [        
            new TriggerAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            new TrackAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
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
        this.Metadata.FabricSparkJobBatchId = string.Empty;
        this.Metadata.SparkPoolId = string.Empty;
        this.Metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.FabricSparkJobStatus = DataPlaneSparkJobStatus.Others;
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

