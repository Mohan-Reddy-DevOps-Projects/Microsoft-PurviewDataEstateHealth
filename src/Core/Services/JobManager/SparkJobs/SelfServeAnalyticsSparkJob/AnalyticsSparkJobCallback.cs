namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.SelfServeAnalyticsSparkJob;

using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DEH.Domain.LogAnalytics;

[JobCallback(Name = nameof(AnalyticsSparkJobCallback))]
internal class AnalyticsSparkJobCallback(IServiceScope scope) : StagedWorkerJobCallback<DataPlaneSparkJobMetadata>(scope)
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly ISparkJobManager sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();
    private readonly IServiceScope serviceScope = scope;
    private readonly IMetadataAccessorService _metadataAccessorService = scope.ServiceProvider.GetService<IMetadataAccessorService>();
    private readonly IDEHAnalyticsJobLogsRepository _dehAnalyticsJobLogsRepository = scope.ServiceProvider.GetService<IDEHAnalyticsJobLogsRepository>();
    private readonly IAccountExposureControlConfigProvider _exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
 
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

    private async Task<bool> IsCatalogJobRunningAsync()
    {
        try
        {
            if (this._dehAnalyticsJobLogsRepository == null)
            {
                this.dataEstateHealthRequestLogger.LogError("DEHAnalyticsJobLogsRepository is not available");
                return false;
            }

            if (this.Metadata.AccountServiceModel == null || string.IsNullOrEmpty(this.Metadata.AccountServiceModel.Id))
            {
                this.dataEstateHealthRequestLogger.LogError("Account information is not available");
                return false;
            }

            // Determine which table to use based on exposure control flag
            string tableName = "DEH_Job_Logs_CL";
            try
            {
                if (this._exposureControl?.IsEnableControlRedesignBillingImprovements(this.Metadata.AccountServiceModel.Id, string.Empty, this.Metadata.AccountServiceModel.TenantId) == true)
                {
                    tableName = "DEH_Job_Logs_V2_CL";
                }
            }
            catch (Exception ex)
            {
                this.dataEstateHealthRequestLogger.LogWarning($"Failed to check exposure control flag, using default table: {ex.Message}");
            }

            // Build KQL query to find running catalog jobs
            var kqlQuery = $@"
                {tableName} 
                | where AccountId_g == '{this.Metadata.AccountServiceModel.Id}'
                | where JobName_s == ""DomainModel""
                | where TimeGenerated > ago(1h)
                | summarize StartCount=countif(JobStatus_s==""Started""), 
                          CompleteCount=countif(JobStatus_s==""Completed"" or JobStatus_s==""Failed"") by JobId_g
                | where StartCount > CompleteCount
                | count";
            
            var result = await this._dehAnalyticsJobLogsRepository.GetDEHJobLogs(kqlQuery, TimeSpan.FromHours(1));
            
            // If count > 0, we have a running job (more starts than completions)
            bool isRunning = result.Count > 0;
            
            this.dataEstateHealthRequestLogger.LogInformation($"Checked for running Catalog jobs. Found running job: {isRunning}");
            
            return isRunning;
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"Error checking for running catalog jobs: {ex.Message}", ex);
            // If we can't determine, assume no jobs are running to avoid deadlock
            return false;
        }
    }

    protected override void OnJobConfigure()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"Configuring job stages for account: {this.Metadata.RequestContext?.AccountId}");
        
        // Set JobRunId for this job run
        this.SetJobId();
        
        try
        {
            // First ensure account info is loaded - needed for checking catalog jobs
            if (this.Metadata.AccountServiceModel == null && this.Metadata.RequestContext != null)
            {
                this.dataEstateHealthRequestLogger.LogInformation($"Loading account info for ID: {this.Metadata.RequestContext.AccountId}");
                var metadataClient = this._metadataAccessorService.GetMetadataServiceClient();
                var accountModel = metadataClient.Accounts.GetAsync(
                    this.Metadata.RequestContext.AccountId.ToString(), "ByAccountId").GetAwaiter().GetResult();
                this.Metadata.AccountServiceModel = accountModel;
                this.dataEstateHealthRequestLogger.LogInformation($"Retrieved account information for {accountModel.Name} (ID: {accountModel.Id})");
            }
            
            // Check if storage sync is configured
            JobSubmissionEvaluator jobSubmissionEvaluator = new JobSubmissionEvaluator(this.serviceScope);
            if (this.Metadata.AccountServiceModel == null || string.IsNullOrEmpty(this.Metadata.AccountServiceModel.Id) || string.IsNullOrEmpty(this.Metadata.AccountServiceModel.TenantId))
            {
                this.dataEstateHealthRequestLogger.LogError("Account information missing required fields (ID or TenantId)");
                this.JobStages = [];
                return;
            }
            
            var isStorageSyncConfigured = jobSubmissionEvaluator.IsStorageSyncConfigured(
                this.Metadata.AccountServiceModel.Id,
                this.Metadata.AccountServiceModel.TenantId).GetAwaiter().GetResult();
                
            this.dataEstateHealthRequestLogger.LogInformation($"Storage sync configured: {isStorageSyncConfigured}");
                
            // For AnalyticsSparkJobCallback, we only check if storage sync is configured without checking its status
            if (!isStorageSyncConfigured)
            {
                this.dataEstateHealthRequestLogger.LogInformation("Storage sync not configured, skipping all stages");
                this.JobStages = [];
                return;
            }
            
            // Check if a catalog job is currently running
            bool isCatalogJobRunning = false;
            try
            {
                isCatalogJobRunning = this.IsCatalogJobRunningAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                this.dataEstateHealthRequestLogger.LogError($"Error checking for running catalog jobs: {ex.Message}", ex);
                // Continue with execution if we can't determine whether catalog job is running
            }
            
            var stages = new List<IJobCallbackStage>();
            
            if (isCatalogJobRunning)
            {
                // Configure retry parameters
                int maxRetries = 3; // Initial attempt + 2 retries
                int delayMinutes = 30;
                var firstDelayTime = DateTime.UtcNow.AddMinutes(delayMinutes);
                
                this.dataEstateHealthRequestLogger.LogInformation(
                    $"Catalog job is running, adding delay stage with {maxRetries-1} retries, first delay until {firstDelayTime:yyyy-MM-dd HH:mm:ss}");
                
                // Add delay stage with retry configuration
                stages.Add(new SimpleDelayStage(
                    this.JobCallbackUtils, 
                    firstDelayTime, 
                    currentAttempt: 1, 
                    maxAttempts: maxRetries, 
                    delayMinutes: delayMinutes));
            }
            
            // Check if DEH ran in the last 24 hours
            var isDEHRanInLast24Hours = jobSubmissionEvaluator.IsDEHRanInLast24Hours(this.Metadata.AccountServiceModel?.Id).GetAwaiter().GetResult();
            
            this.dataEstateHealthRequestLogger.LogInformation($"DEH job ran in last 24 hours: {isDEHRanInLast24Hours}");
            
            // Add analytics stages
            stages.Add(new TriggerAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
            stages.Add(new TrackAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
            
            // If DEH didn't run in the last 24 hours, add catalog and dimension stages
            if (!isDEHRanInLast24Hours)
            {
                this.dataEstateHealthRequestLogger.LogInformation("DEH job did not run in last 24 hours, adding Catalog and Dimension model stages");
                // Insert catalog and dimension stages before analytics stages
                stages.Insert(stages.Count - 2, new TriggerCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
                stages.Insert(stages.Count - 2, new TrackCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
                stages.Insert(stages.Count - 2, new TriggerDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
                stages.Insert(stages.Count - 2, new TrackDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
            }
            
            this.JobStages = stages.ToArray();
            this.dataEstateHealthRequestLogger.LogInformation($"Configured job with {stages.Count} stages");
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"Error configuring job stages: {ex.Message}", ex);
            this.JobStages = [        
                new TriggerAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TrackAnalyticsSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            ];
        }
    }
    
    // Simplified delay stage with minimal implementation
    private class SimpleDelayStage : IJobCallbackStage
    {
        private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;
        private readonly DateTime nextExecutionTime;
        private readonly int currentAttempt;
        private readonly int maxAttempts;
        private readonly int delayMinutes;
        
        public SimpleDelayStage(
            JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils, 
            DateTime nextExecutionTime, 
            int currentAttempt = 1, 
            int maxAttempts = 3, 
            int delayMinutes = 30)
        {
            this.jobCallbackUtils = jobCallbackUtils;
            this.nextExecutionTime = nextExecutionTime;
            this.currentAttempt = currentAttempt;
            this.maxAttempts = maxAttempts;
            this.delayMinutes = delayMinutes;
        }
        
        public string StageName => $"DelayStage_Attempt{this.currentAttempt}";
        
        public Task<JobExecutionResult> Execute()
        {
            string message = $"Catalog job running - delay attempt {this.currentAttempt}/{this.maxAttempts} until {this.nextExecutionTime:yyyy-MM-dd HH:mm:ss}";
            
            // If not the last attempt, create a new stage for next attempt
            if (this.currentAttempt < this.maxAttempts)
            {
                var nextAttemptTime = DateTime.UtcNow.AddMinutes(this.delayMinutes);
                var nextStage = new SimpleDelayStage(
                    this.jobCallbackUtils, 
                    nextAttemptTime,
                    this.currentAttempt + 1,
                    this.maxAttempts,
                    this.delayMinutes);
                
                // Set next execution time to retry
                return Task.FromResult(this.jobCallbackUtils.GetExecutionResult(
                    JobExecutionStatus.Completed,
                    message,
                    this.nextExecutionTime));
            }
            
            // Last attempt - proceed with execution
            return Task.FromResult(this.jobCallbackUtils.GetExecutionResult(
                JobExecutionStatus.Completed,
                $"{message} - Maximum retries reached, proceeding with execution.",
                null)); // No delay for the next stage
        }
        
        public bool IsStageComplete() => true;
        
        public bool IsStagePreconditionMet() => true;
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
        
        // Reset the job id
        this.Metadata.JobRunId = string.Empty;
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

    private void SetJobId()
    {
        if (String.IsNullOrEmpty(this.Metadata.JobRunId))
        {
            this.Metadata.JobRunId = Guid.NewGuid().ToString();
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Generated new JobRunId: {this.Metadata.JobRunId}");
        }
        else
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Using existing JobRunId: {this.Metadata.JobRunId}");
        }
    }
}

