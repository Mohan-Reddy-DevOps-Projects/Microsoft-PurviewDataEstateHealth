// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TrackTriggeredControlsDqRulesEvaluationSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils;
    private readonly DehScheduleJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IControlsWorkflowSparkJobComponent controlsWorkflowSparkJobComponent;

    public TrackTriggeredControlsDqRulesEvaluationSparkJobStage(
    IServiceScope scope,
    DehScheduleJobMetadata metadata,
    JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.controlsWorkflowSparkJobComponent = scope.ServiceProvider.GetService<IControlsWorkflowSparkJobComponent>();
        scope.ServiceProvider.GetService<IJobManager>();
    }

    public string StageName => nameof(TrackTriggeredControlsDqRulesEvaluationSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        var jobStageStatus = JobExecutionStatus.Postponed;
        using (this.logger.LogElapsed($"Track controls workflow spark job"))
        {
            string jobStatusMessage;
            try
            {
                SparkBatchJob jobDetails;

                if (String.IsNullOrEmpty(this.metadata.CatalogSparkJobMetadata.SparkPoolId))
                {
                    // per-account pre-provisioned pool
                    jobDetails = await this.controlsWorkflowSparkJobComponent.GetJob(
                        this.metadata.CatalogSparkJobMetadata.AccountServiceModel,
                        Int32.Parse(this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId),
                        CancellationToken.None);
                }
                else
                {
                    // per-job pool
                    var jobInfo = new SparkPoolJobModel()
                    {
                        JobId = this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId,
                        PoolResourceId = this.metadata.CatalogSparkJobMetadata.SparkPoolId
                    };
                    jobDetails = await this.controlsWorkflowSparkJobComponent.GetJob(
                        jobInfo, CancellationToken.None);
                }

                this.logger.LogTrace($"Controls Workflow Spark job status: state, {jobDetails.State}, result, {jobDetails.Result}");
                if (SparkJobUtils.IsSuccess(jobDetails))
                {
                    this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus = ControlsWorkflowStatus.Succeeded;
                    jobStageStatus = JobExecutionStatus.Completed;
                }
                else if (SparkJobUtils.IsFailure(jobDetails))
                {
                    this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus = ControlsWorkflowStatus.Failed;
                    jobStageStatus = JobExecutionStatus.Failed;
                }

                jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.CatalogSparkJobMetadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
                this.logger.LogTrace($"Track controls workflow spark job stage status: {jobStatusMessage}");
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"{this.StageName}|Failed to track Controls Workflow SPARK job for account: {this.metadata.CatalogSparkJobMetadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
                this.logger.LogError(jobStatusMessage, exception);
            }

            var result = this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));

            return result;
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus is ControlsWorkflowStatus.Succeeded or ControlsWorkflowStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return Int32.TryParse(this.metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId, out int _);
    }
} 