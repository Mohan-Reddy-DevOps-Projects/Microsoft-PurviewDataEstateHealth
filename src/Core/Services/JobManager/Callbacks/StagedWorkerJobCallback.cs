// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Staged Job Callback for background jobs
/// </summary>
internal abstract class StagedWorkerJobCallback<TMetadata> : JobCallback<TMetadata>
    where TMetadata : StagedWorkerJobMetadata
{
    private readonly IRequestContextAccessor requestContextAccessor;

    /// <summary>
    /// Indicate if job pre-condition(s) met.
    /// </summary>
    protected bool IsJobPreconditionValid;

    /// <summary>
    /// Utility helpers for job and stages.
    /// </summary>
    protected JobCallbackUtils<TMetadata> JobCallbackUtils;

    /// <summary>
    /// Stages that make up this job.
    /// </summary>
    protected IEnumerable<IJobCallbackStage> JobStages;

    /// <summary>
    /// Scope for job stage management.
    /// </summary>
    protected IServiceScope Scope;

    /// <summary>
    /// Logger for job callbacks.
    /// </summary>
    protected IDataEstateHealthRequestLogger DataEstateHealthRequestLogger;

    /// <summary>
    /// Used to generate traces
    /// </summary>
    protected Activity TraceActivity;

    /// <summary>
    /// Instantiate instance of StagedWorkerJobCallback.
    /// </summary>
    protected StagedWorkerJobCallback()
    {
    }

    /// <summary>
    /// Instantiate instance of StagedWorkerJobCallback.
    /// </summary>
    /// <param name="scope"></param>
    protected StagedWorkerJobCallback(IServiceScope scope)
    {
        this.Scope = scope;
        this.DataEstateHealthRequestLogger = scope.ServiceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        this.requestContextAccessor = scope.ServiceProvider.GetRequiredService<IRequestContextAccessor>();
    }

    /// <summary>
    /// Gets the job's maximum retry count. When this is exceeded, Job status will transition to faulted even if the current
    /// execution produces a retry-able error.
    /// </summary>
    protected virtual int MaxRetryCount => 5;

    /// <summary>
    /// Specifies whether the job is recurring or not.
    ///
    /// A recurring job runs every day at the same time. The logic for postpone count checks is different for
    /// recurring jobs so we need some way to identify them. Since the vast majority of our jobs are not recurring,
    /// this defaults to false.
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsRecurringJob => false;

    /// <summary>
    /// Gets the current schedule start time.
    /// For example, If the job is set to be executed every 2 hours, this value is the start time of each schedule execution.
    /// </summary>
    protected virtual DateTime CurrentJobScheduleStartTime { get; }

    /// <summary>
    /// The job name
    /// </summary>
    protected abstract string JobName { get; }

    /// <summary>
    /// Perform job configuration steps
    /// </summary>
    /// <returns></returns>
    protected sealed override Task OnConfigure()
    {
        var otelInstrumentation = this.Scope.ServiceProvider.GetService<IOtelInstrumentation>();

        this.JobCallbackUtils = new JobCallbackUtils<TMetadata>(this.Metadata);
        string jobName = this.GetType().Name.Replace("Callback", string.Empty);

        this.TraceActivity = otelInstrumentation?.ActivitySource.StartActivity($"{jobName}-{this.BackgroundJob.JobId}",
            ActivityKind.Consumer);

        if (this.TraceActivity != null)
        {
            if (!String.IsNullOrEmpty(this.Metadata.TraceId))
            {
                var parentTrace = ActivityTraceId.CreateFromString(this.Metadata.TraceId);
                var parentSpan = ActivitySpanId.CreateFromString(this.Metadata.SpanId);

                this.TraceActivity.SetParentId(parentTrace, parentSpan, ActivityTraceFlags.Recorded);
            }

            if (!String.IsNullOrEmpty(this.Metadata.RootTraceId))
            {
                this.TraceActivity.SetRootIdTag(this.Metadata.RootTraceId);
            }

            this.TraceActivity.SetTag("TenantId", this.Metadata.RequestContext.TenantId);
            this.TraceActivity.SetTag("AccountId", this.Metadata.RequestContext.AccountId);
            this.TraceActivity.SetTag("ApiVersion", this.Metadata.RequestContext.ApiVersion);
            this.TraceActivity.SetTag("CorrelationId", this.Metadata.RequestContext.CorrelationId);
            this.TraceActivity.IsAllDataRequested = true;
            this.TraceActivity.ActivityTraceFlags = ActivityTraceFlags.Recorded;
        }

        this.OnJobConfigure();

        return Task.CompletedTask;
    }

    /// <summary>
    /// execute
    /// </summary>
    /// <returns></returns>
    protected sealed override async Task<JobExecutionResult> OnExecute()
    {
        return await this.OnJobExecute().ConfigureAwait(false);
    }

    /// <summary>
    /// On Execution Result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    protected sealed override async Task<JobExecutionResult> OnExecutionResult(
        JobExecutionResult result,
        Exception exception)
    {
        return await this.OnJobExecutionResult(result, exception).ConfigureAwait(false);
    }

    /// <summary>
    /// Configure job settings e.g. job stages
    /// </summary>
    /// <returns></returns>
    protected abstract void OnJobConfigure();

    /// <summary>
    /// Check if job's pre-conditions are met
    /// </summary>
    /// <returns></returns>
    protected abstract Task<bool> IsJobPreconditionMet();

    /// <summary>
    /// Transition job to succeeded state when all stages succeed
    /// </summary>
    /// <returns></returns>
    protected abstract Task TransitionToJobSucceeded();

    /// <summary>
    /// Transition job to failed if any of the stages fail
    /// </summary>
    /// <returns></returns>
    protected abstract Task TransitionToJobFailed();

    /// <summary>
    /// Final steps to execute post job irrespective of outcome
    /// </summary>
    /// <returns></returns>
    protected abstract Task FinalizeJob(JobExecutionResult result, Exception exception);

    /// <summary>
    /// Check if job reach max excution time
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsJobReachMaxExecutionTime()
    {
        return false;
    }

    /// <summary>
    /// On Job Execution Result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    protected virtual async Task<JobExecutionResult> OnJobExecutionResult(
        JobExecutionResult result,
        Exception exception)
    {
        using (this.DataEstateHealthRequestLogger.LogElapsed($"Execute job result, name: {this.JobName}"))
        {
            try
            {
                this.SetRequestContext();

                this.DataEstateHealthRequestLogger.LogInformation($"Job execution result: {result.Status}|{result.Message}");

                if (exception != null)
                {
                    this.DataEstateHealthRequestLogger.LogError("Job failed", exception, operationName: this.GetType().Name);
                }

                this.DataEstateHealthRequestLogger.LogInformation($"Current TotalFailedCount: {this.BackgroundJob.TotalFailedCount}, MaxRetryCount: {this.MaxRetryCount}, CurrentRetryCount: {this.BackgroundJob.CurrentRetryCount}, CurrentRepeatCount: {this.BackgroundJob.CurrentRepeatCount}");

                if (result.Status == JobExecutionStatus.Failed && this.BackgroundJob.CurrentRetryCount >= this.MaxRetryCount)
                {
                    this.DataEstateHealthRequestLogger.LogInformation($"Job failed with {this.BackgroundJob.CurrentRetryCount} retries");
                    await this.TransitionToJobFailed();

                    if (this.IsRecurringJob)
                    {
                        var errorMessage = $"Recurring job {this.JobName} has exhausted the maximum job retries of {this.MaxRetryCount}";
                        this.DataEstateHealthRequestLogger.LogError(
                            errorMessage,
                            exception,
                            operationName: this.GetType().Name);

                        return new JobExecutionResult
                        {
                            Status = JobExecutionStatus.Succeeded,  //need to return as succeeded to wait for next schedule
                            Message = errorMessage,
                            NextMetadata = this.Metadata.ToString()
                        };
                    }
                    else
                    {
                        this.DataEstateHealthRequestLogger.LogError(
                            FormattableString.Invariant(
                                $"job has exhausted the maximum job retries of {this.MaxRetryCount}"),
                            exception,
                            operationName: this.GetType().Name);

                        result = this.JobCallbackUtils.FaultJob(
                            ErrorCode.Job_MaximumRetryCount,
                            "Exhausted retry count.");
                    }
                }

                if (result.Status == JobExecutionStatus.Postponed && this.IsJobReachMaxExecutionTime())
                {
                    await this.TransitionToJobFailed();

                    var errorMessage = $"Job {this.JobName} has reached max execution time";

                    if (this.IsRecurringJob)
                    {
                        this.DataEstateHealthRequestLogger.LogError(
                            errorMessage,
                            exception,
                            operationName: this.GetType().Name);

                        return new JobExecutionResult
                        {
                            Status = JobExecutionStatus.Succeeded,  //need to return as succeeded to wait for next schedule
                            Message = errorMessage,
                            NextMetadata = this.Metadata.ToString()
                        };
                    }
                    else
                    {
                        this.DataEstateHealthRequestLogger.LogError(
                            errorMessage,
                            exception,
                            operationName: this.GetType().Name);

                        result = this.JobCallbackUtils.FaultJob(
                            ErrorCode.Job_MaximumPostponeCount,
                            errorMessage);
                    }
                }

                if (this.Metadata?.ServiceExceptions != null)
                {
                    foreach (ServiceException serviceException in this.Metadata.ServiceExceptions)
                    {
                        string errorMessage = $"Error encountered in job with ID '{this.JobId}'";

                        // Only log a critical when the error isn't a client error and the job is faulted. Otherwise, we
                        // can get an IcM for a job that ended up succeeding.
                        if (!this.IsJobPreconditionValid ||
                            result.Status != JobExecutionStatus.Faulted)
                        {
                            this.DataEstateHealthRequestLogger.LogError(errorMessage, serviceException, operationName: this.GetType().Name);
                        }
                        else
                        {
                            this.DataEstateHealthRequestLogger.LogCritical(errorMessage, serviceException, operationName: this.GetType().Name);
                        }
                    }
                }

                if (result.Status == JobExecutionStatus.Faulted && this.IsJobPreconditionValid)
                {
                    await this.TransitionToJobFailed();
                    //Need to update NextMetadata after reset.
                    result.NextMetadata = this.Metadata.ToJson();
                }

                await this.FinalizeJob(result, exception);
            }
            finally
            {
                this.TraceActivity?.Dispose();
            }

            return result;
        }
    }

    /// <summary>
    /// On Job execute
    /// </summary>
    /// <returns></returns>
    private async Task<JobExecutionResult> OnJobExecute()
    {
        this.SetRequestContext();

        using (this.DataEstateHealthRequestLogger.LogElapsed($"Run job, name: {this.JobName}"))
        {
            this.IsJobPreconditionValid = await this.IsJobPreconditionMet();
            if (!this.IsJobPreconditionValid)
            {
                this.DataEstateHealthRequestLogger.LogInformation($"{this.JobName} | Current job pre-condition is not met");

                return this.JobCallbackUtils.GetExecutionResult(
                    JobExecutionStatus.Failed,
                    $"{this.JobName}  | Current job pre-condition is not met");
            }
            foreach (IJobCallbackStage stage in this.JobStages)
            {
                this.DataEstateHealthRequestLogger.LogInformation($"Start to run {stage.StageName}");
                if (stage.IsStageComplete())
                {
                    this.DataEstateHealthRequestLogger.LogInformation($"{stage.StageName} already completed, skipped");
                    continue;
                }

                JobExecutionResult jobResult = null;

                try
                {
                    await stage.InitializeStage();

                    if (!stage.IsStagePreconditionMet())
                    {
                        this.DataEstateHealthRequestLogger.LogInformation($"{stage.StageName} | Current stage pre-condition is not met");
                        return this.JobCallbackUtils.GetExecutionResult(
                            JobExecutionStatus.Failed,
                            $"{stage.StageName} | Current stage pre-condition is not met");
                    }

                    this.DataEstateHealthRequestLogger.LogInformation(
                        $"Executing {stage.StageName} with context: {string.Join(",", this.Metadata.WorkerJobExecutionContext.GetFlags())}");

                    jobResult = await stage.Execute();

                    this.DataEstateHealthRequestLogger.LogInformation(
                        $"Executed {stage.StageName} with result: {jobResult.Status}|{jobResult.Message}");
                }
                catch (ServiceException serviceException)
                {
                    this.JobCallbackUtils.AugmentServiceException(serviceException);

                    return this.LogAndConvert(serviceException, stage.StageName);
                }
                catch (AggregateException aggregateException)
                {
                    foreach (Exception innerException in aggregateException.InnerExceptions)
                    {
                        if (innerException is ServiceException serviceException)
                        {
                            this.JobCallbackUtils.AugmentServiceException(serviceException);
                        }
                    }

                    return this.LogAndConvert(aggregateException, stage.StageName);
                }
                catch (Exception exception)
                {
                    return this.LogAndConvert(exception, stage.StageName);
                }

                if (jobResult?.Status != JobExecutionStatus.Completed)
                {
                    return jobResult;
                }
            }
            this.DataEstateHealthRequestLogger.LogInformation($"All stage complete, job name: {this.JobName}");

            await this.TransitionToJobSucceeded();

            return this.JobCallbackUtils.GetExecutionResult(
                JobExecutionStatus.Succeeded,
                $"{this.JobName} executed successfully");
        }
    }

    private JobExecutionResult LogAndConvert(Exception exception, string stageName)
    {
        string log = $"Exception in {stageName}. {exception.Message}";
        this.DataEstateHealthRequestLogger.LogError(log, exception);

        return this.JobCallbackUtils.GetExecutionResult(JobExecutionStatus.Failed, log);
    }

    /// <summary>
    /// Condition that determines if a job has exceeded its defined execution parameters (retry or postpone count).
    /// This guard clause prevents jobs from entering infinite loops.
    /// </summary>
    /// <returns>True if the job is outside the defined execution parameters; otherwise false.</returns>
    private bool HasExceededExecutionConstraints()
    {
        return this.BackgroundJob.TotalFailedCount > this.MaxRetryCount
            || this.IsJobReachMaxExecutionTime();
    }

    private void SetRequestContext()
    {
        IRequestContext context = this.requestContextAccessor.GetRequestContext().WithCallbackContext(this.Metadata.RequestContext);

        // Set correlation-id if not set in the payload.
        if (string.IsNullOrEmpty(this.Metadata.RequestContext.CorrelationId))
        {
            Guid correlationId = Guid.NewGuid();
            context.SetCorrelationIdInRequestContext(correlationId.ToString());

            this.Metadata.RequestContext.CorrelationId = correlationId.ToString();
        }

        this.requestContextAccessor.SetRequestContext(context);
    }
}
