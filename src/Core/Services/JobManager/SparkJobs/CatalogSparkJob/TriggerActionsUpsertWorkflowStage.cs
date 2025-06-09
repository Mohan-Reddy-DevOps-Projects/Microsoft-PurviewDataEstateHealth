// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class TriggerActionsUpsertWorkflowStage(
    IServiceScope scope,
    DehScheduleJobMetadata metadata,
    JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils) : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly IDataHealthApiService dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
    private readonly DhControlJobRepository dhControlJobRepository = scope.ServiceProvider.GetService<DhControlJobRepository>();

    public string StageName => nameof(TriggerActionsUpsertWorkflowStage);

    public async Task<JobExecutionResult> Execute()
    {
        string accountId = metadata.CatalogSparkJobMetadata.AccountServiceModel.Id;
        string tenantId = metadata.CatalogSparkJobMetadata.AccountServiceModel.TenantId;
        
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger actions generation workflow"))
        {
            JobExecutionStatus jobStageStatus;
            string jobStatusMessage;
            try
            {
                this.dataEstateHealthRequestLogger.LogInformation($"Triggering actions creation for account: {accountId}, tenant: {tenantId}, controlsWorkflowJobId: {metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId}");

                // Fetch all controls with the specified jobId
                var controlJobs = await this.dhControlJobRepository.GetControlJobsByJobIdAsyncForAccountAndTenant(metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId, accountId, tenantId).ConfigureAwait(false);
                
                if (controlJobs.Count == 0)
                {
                    jobStageStatus = JobExecutionStatus.Completed;
                    jobStatusMessage = $"No control jobs found for jobId: {metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId}. Setting job as completed.";
                    this.dataEstateHealthRequestLogger.LogInformation(jobStatusMessage);
                    
                    // Set the status to succeeded when no controls found (successful completion)
                    metadata.ActionsGenerationMetadata.ActionsGenerationWorkflowStatus = ActionsGenerationWorkflowStatus.Succeeded;
                    
                    return jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
                }

                var controlIds = controlJobs.SelectMany(job => job.Evaluations).Select(evaluation => evaluation.ControlId).ToList();
                this.dataEstateHealthRequestLogger.LogInformation($"Found {controlJobs.Count} control jobs with IDs: {String.Join(", ", controlIds)}");

                // Trigger actions creation for each control individually
                foreach (var mdqJobModel in controlIds.Select(controlId => new Models.MDQJobModel
                         {
                             AccountId = new Guid(accountId),
                             TenantId = new Guid(tenantId),
                             DQJobId = new Guid(metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId),
                             ControlId = controlId,
                             JobStatus = "Succeeded"
                         }))
                {
                    await this.dataHealthApiService.TriggerActionsUpsert(
                        mdqJobModel,
                        metadata.CatalogSparkJobMetadata.TraceId,
                        CancellationToken.None).ConfigureAwait(false);
                    
                    this.dataEstateHealthRequestLogger.LogInformation($"Triggered actions creation for control: {mdqJobModel.ControlId}");
                }
                
                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"Actions creation workflow triggered successfully for account: {accountId}, processed {controlIds.Count} controls with jobId: {metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId}";
                this.dataEstateHealthRequestLogger.LogInformation(jobStatusMessage);
                
                // Set the status to succeeded when stage completes successfully
                metadata.ActionsGenerationMetadata.ActionsGenerationWorkflowStatus = ActionsGenerationWorkflowStatus.Succeeded;
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to trigger actions creation workflow for account: {accountId}, jobId: {metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId} with error: {exception.Message}";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
                
                // Set the status to failed when stage fails
                metadata.ActionsGenerationMetadata.ActionsGenerationWorkflowStatus = ActionsGenerationWorkflowStatus.Failed;
            }

            return jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        // Stage is complete if it has succeeded
        return metadata.ActionsGenerationMetadata.ActionsGenerationWorkflowStatus == ActionsGenerationWorkflowStatus.Succeeded;
    }

    public bool IsStagePreconditionMet()
    {
        // This stage should execute after the TrackTriggeredControlsDqRulesEvaluationSparkJobStage stage
        // Ensure we have a valid batch ID from the controls workflow
        return metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus == ControlsWorkflowStatus.Succeeded;
    }
}
