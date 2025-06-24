// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading;
using System.Threading.Tasks;

internal class TriggerControlsDqRulesEvaluationSparkJobStage(
    IServiceScope scope,
    DehScheduleJobMetadata metadata,
    JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils) : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly IAccountExposureControlConfigProvider exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    private readonly IControlsWorkflowSparkJobComponent controlsWorkflowSparkJobComponent = scope.ServiceProvider.GetService<IControlsWorkflowSparkJobComponent>();

    public string StageName => nameof(TriggerControlsDqRulesEvaluationSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        string jobId = metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId;
        string accountId = metadata.CatalogSparkJobMetadata.AccountServiceModel.Id;
        string tenantId = metadata.CatalogSparkJobMetadata.AccountServiceModel.TenantId;
        bool isDehDataCleanup = this.exposureControl.IsDEHDataCleanup(accountId, String.Empty, tenantId);
        
        this.dataEstateHealthRequestLogger.LogInformation($"Using Controls Workflow Job ID: {jobId} for account: {accountId}");
        
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger controls workflow spark job"))
        {
            string jobStatusMessage;
            JobExecutionStatus jobStageStatus;
            try
            {
                // Set metadata values to track job status
                metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus = ControlsWorkflowStatus.Submitting;
                
                // Submit the job using the component
                var jobInfo = await this.controlsWorkflowSparkJobComponent.SubmitJob(
                    metadata.CatalogSparkJobMetadata.AccountServiceModel,
                    CancellationToken.None,
                    jobId,
                    metadata.CatalogSparkJobMetadata.SparkPoolId,
                    isDehDataCleanup,
                    metadata.JobRunId);
                
                // Update the metadata with job information
                metadata.CatalogSparkJobMetadata.SparkPoolId = jobInfo.PoolResourceId;
                metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId = jobInfo.JobId;
                
                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"Controls Workflow job submitted for account: {accountId} in {this.StageName}, JobID: {jobId}, BatchID: {jobInfo.JobId}";
                this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to submit Controls Workflow job for account: {accountId} in {this.StageName} with error: {exception.Message}, JobID: {jobId}";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
            }

            return jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        return Int32.TryParse(metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId, out int _);
    }

    public bool IsStagePreconditionMet()
    {
        // This stage should execute after the SQL generation
        return metadata.SqlGenerationMetadata.SqlGenerationStatus == SqlGenerationWorkflowStatus.Succeeded;
    }
}