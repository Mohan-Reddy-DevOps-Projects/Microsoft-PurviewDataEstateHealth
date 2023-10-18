// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Polly;
using System.Collections.Concurrent;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;

/// <inheritdoc />
public abstract class JobManager : IJobManager
{
    /// <summary>
    /// Default retry interval
    /// </summary>
    protected const long DefaultRetryInterval = 60;

    /// <summary>
    /// Job ID format
    /// </summary>
    private static readonly string jobIdFormat = $"{{0}}-{{1}}-{{2}}";

    /// <summary>
    /// Job management client for managing jobs
    /// </summary>
    protected Lazy<Task<JobManagementClient>> JobManagementClient;

    /// <summary>
    /// Request header context
    /// </summary>
    protected RequestHeaderContext RequestHeaderContext;

    /// <summary>
    /// Logger
    /// </summary>
    protected readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <summary>
    /// The Execution Context for the Job Manager.
    /// </summary>
    protected readonly WorkerJobExecutionContext WorkerJobExecutionContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobManager" /> class.
    /// </summary>
    public JobManager(
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IOptions<JobConfiguration> jobConfiguration,
        IStorageCredentialsProvider storageCredentialsProvider,
        WorkerJobExecutionContext workerJobExecutionContext = WorkerJobExecutionContext.None)
    {
        var dataConsistencyOptions = new StorageConsistencyOptions
        {
            ShardingEnabled = false,
            ReplicationEnabled = false
        };

        this.JobManagementClient = new Lazy<Task<JobManagementClient>>(async () =>
        {
            StorageCredentials storageCredentials = await storageCredentialsProvider.GetCredentialsAsync();
            var cloudStorageAccount = new CloudStorageAccount(
                storageCredentials: storageCredentials,
                jobConfiguration.Value.StorageAccountName,
                endpointSuffix: environmentConfiguration.Value.AzureEnvironment.StorageEndpointSuffix,
                useHttps: true);

            return new JobManagementClient(
                storageAccount: cloudStorageAccount,
                executionAffinity: environmentConfiguration.Value.Location,
                // TODO(zachmadsen): Pass the job logger here.
                eventSource: null,
                tableName: "jobdefinitions",
                queueNamePrefix: "jobtriggers",
                requestOptions: null,
                consistencyOptions: dataConsistencyOptions,
                secretThumbprint: null,
                compressionUtility: null,
                notificationChannel: null);
        });

        this.RequestHeaderContext = (RequestHeaderContext)requestHeaderContext;
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
        this.WorkerJobExecutionContext = workerJobExecutionContext;
    }

    /// <inheritdoc />
    public string BuildJobId(string operationType, string resourceType, Guid resourceId)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            JobManager.jobIdFormat,
            operationType,
            resourceType,
            resourceId);
    }

    /// <inheritdoc />
    public async Task DeleteJobAsync(string partitionId, string jobId)
    {
        JobManagementClient jobClient = await this.JobManagementClient.Value;

        await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.dataEstateHealthRequestLogger, nameof(JobManager)))
            .ExecuteAsync(() => jobClient.DeleteJob(
                partitionId,
                jobId));
    }

    /// <inheritdoc />
    public async Task<JobExecutionStatus> GetJobsStatusAsync(string partitionId, List<string> jobIds)
    {
        var jobStatusTasks = new List<Task<JobExecutionStatus>>();

        jobIds = jobIds.Where(jobId => jobId != Guid.Empty.ToString()).ToList();

        foreach (string jobId in jobIds)
        {
            jobStatusTasks.Add(
                Task.Run(
                    async () =>
                    {
                        BackgroundJob backgroundJob = await this.GetJobAsync(partitionId, jobId);

                        if (backgroundJob is null)
                        {
                            this.dataEstateHealthRequestLogger.LogCritical(
                                $"GetJobsStatusAsync: job with ID '{jobId}' under partition {partitionId} was not found.",
                                null);
                        }

                        return this.ReduceJobExecutionStatus(backgroundJob);
                    }));
        }

        JobExecutionStatus[] jobStatuses = await Task.WhenAll(jobStatusTasks);
        return this.ReduceJobExecutionStatuses(jobStatuses);
    }

    /// <inheritdoc />
    public async Task<AggregateJobExecutionStatus> GetJobsAndExecutionStatusAsync(string partitionId, List<string> jobIds)
    {
        var jobStatusTasks = new List<Task<JobExecutionStatus>>();
        var jobExecutionStatusResult = new AggregateJobExecutionStatus();

        jobIds = jobIds.Where(jobId => jobId != Guid.Empty.ToString()).ToList();

        jobExecutionStatusResult.JobsStatuses = new ConcurrentDictionary<string, JobExecutionStatus?>();

        foreach (string jobId in jobIds)
        {
            jobStatusTasks.Add(
                Task.Run(
                    async () =>
                    {
                        BackgroundJob backgroundJob = await this.GetJobAsync(partitionId, jobId);

                        jobExecutionStatusResult.JobsStatuses[jobId] = backgroundJob?.LastExecutionStatus;

                        if (backgroundJob is null)
                        {
                            this.dataEstateHealthRequestLogger.LogError(
                                $"GetJobsStatusAsync: job with ID '{jobId}' under partition {partitionId} was not found.",
                                null);
                        }

                        return this.ReduceJobExecutionStatus(backgroundJob);
                    }));
        }

        JobExecutionStatus[] jobStatuses = await Task.WhenAll(jobStatusTasks);
        jobExecutionStatusResult.JobExecutionStatus = this.ReduceJobExecutionStatuses(jobStatuses);
        return jobExecutionStatusResult;
    }

    /// <inheritdoc/>
    public JobExecutionStatus ReduceJobExecutionStatus(BackgroundJob backgroundJob)
    {
        if (backgroundJob?.State == JobState.Disabled ||
            backgroundJob?.State == JobState.Enabled ||
            backgroundJob?.State == JobState.Suspended)
        {
            return JobExecutionStatus.Postponed;
        }

        if (backgroundJob?.LastExecutionStatus != null)
        {
            return backgroundJob.LastExecutionStatus.Value;
        }

        // default to postponed
        return JobExecutionStatus.Postponed;
    }

    /// <inheritdoc />
    public JobExecutionStatus ReduceJobExecutionStatuses(JobExecutionStatus[] jobStatuses)
    {
        if (jobStatuses.Any(status =>
                status == JobExecutionStatus.Postponed ||
                status == JobExecutionStatus.Failed ||
                status == JobExecutionStatus.Rescheduled))
        {
            return JobExecutionStatus.Postponed;
        }

        if (jobStatuses.Any(status => status == JobExecutionStatus.Faulted))
        {
            return JobExecutionStatus.Faulted;
        }

        return JobExecutionStatus.Succeeded;
    }

    /// <inheritdoc />
    public async Task<BackgroundJob[]> GetJobsAsync(string partitionId)
    {
        JobManagementClient jobClient = await this.JobManagementClient.Value;

        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.dataEstateHealthRequestLogger, nameof(JobManager)))
            .AsAsyncPolicy<BackgroundJob[]>()
            .ExecuteAsync(() => jobClient.GetJobs(partitionId));
    }

    /// <inheritdoc />
    public async Task<BackgroundJob> GetJobAsync(string partitionId, string jobId)
    {
        JobManagementClient jobClient = await this.JobManagementClient.Value;

        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.dataEstateHealthRequestLogger, nameof(JobManager)))
            .AsAsyncPolicy<BackgroundJob>()
            .ExecuteAsync(() => jobClient.GetJob(partitionId, jobId));
    }

    /// <inheritdoc />
    public async Task<BackgroundJob[]> GetJobsAsync(string partitionId, List<string> jobIds)
    {
        return await Task.WhenAll(
            jobIds
                .Where(jobId => jobId != Guid.Empty.ToString())
                .Select(async jobId => await this.GetJobAsync(partitionId, jobId)));
    }

    /// <inheritdoc />
    public async Task<bool> JobMetadataContainsErrorCodeAsync(
        string partitionId,
        string jobId,
        ErrorCode errorCode)
    {
        BackgroundJob backgroundJob = await this.GetJobAsync(partitionId, jobId);
        if (backgroundJob == null)
        {
            return false;
        }

        StagedWorkerJobMetadata metadata;
        try
        {
            metadata = backgroundJob.GetMetadata<StagedWorkerJobMetadata>();
        }
        catch (SerializationException)
        {
            this.dataEstateHealthRequestLogger.LogError(
                $"Failed to deserialize metadata for job '{jobId}' while checking exceptions for an error code.");

            return false;
        }

        foreach (ServiceException exception in metadata.ServiceExceptions)
        {
            if (exception.ServiceError.Code == errorCode.Code)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a job
    /// </summary>
    /// <param name="jobBuilder"></param>
    /// <param name="operationName"></param>
    /// <returns>A Task.</returns>
    protected async Task CreateJobAsync(JobBuilder jobBuilder, [CallerMemberName] string operationName = "")
    {
        try
        {
            JobManagementClient jobClient = await this.JobManagementClient.Value;

            await PollyRetryPolicies
                .GetNonHttpClientTransientRetryPolicy(
                    LoggerRetryActionFactory.CreateWorkerRetryAction(this.dataEstateHealthRequestLogger, nameof(JobManager)))
                .ExecuteAsync(() => jobClient.CreateJob(jobBuilder));

            this.dataEstateHealthRequestLogger.LogInformation(
                FormattableString.Invariant($"Created job for {operationName} with jobId {jobBuilder.JobId}."));
        }
        catch (Exception exception)
        {
            this.dataEstateHealthRequestLogger.LogCritical(
                FormattableString.Invariant($"Creating job failed for {operationName}."),
                exception);

            throw;
        }
    }

    /// <summary>
    /// Gets a job builder with default options.
    /// </summary>
    /// <param name="jobCallbackName"></param>
    /// <param name="jobMetadata"></param>
    /// <param name="partitionId"></param>
    /// <param name="jobId"></param>
    /// <returns>A job builder</returns>
    protected static JobBuilder GetJobBuilderWithDefaultOptions(
        string jobCallbackName,
        StagedWorkerJobMetadata jobMetadata,
        Guid partitionId,
        string jobId = null)
    {
        return JobManager.GetJobBuilderWithOptions(jobCallbackName, jobMetadata, partitionId.ToString(), jobId);
    }

    /// <summary>
    /// Gets a job builder with default options.
    /// </summary>
    /// <param name="jobCallbackName"></param>
    /// <param name="jobMetadata"></param>
    /// <param name="partitionId"></param>
    /// <param name="jobId"></param>
    /// <returns>A job builder</returns>
    protected static JobBuilder GetJobBuilderWithDefaultOptions(
        string jobCallbackName,
        StagedWorkerJobMetadata jobMetadata,
        string partitionId,
        string jobId = null)
    {
        return JobManager.GetJobBuilderWithOptions(jobCallbackName, jobMetadata, partitionId.ToString(), jobId);
    }

    /// <summary>
    /// Gets a job builder with default options.
    /// </summary>
    /// <param name="jobCallbackName"></param>
    /// <param name="jobMetadata"></param>
    /// <param name="partitionId"></param>
    /// <param name="jobId"></param>
    /// <param name="startTime"></param>
    /// <returns>A job builder</returns>
    protected static JobBuilder GetJobBuilderWithOptions(
        string jobCallbackName,
        StagedWorkerJobMetadata jobMetadata,
        Guid partitionId,
        string jobId = null,
        DateTime? startTime = null)
    {
        return JobManager.GetJobBuilderWithOptions(
            jobCallbackName,
            jobMetadata,
            partitionId.ToString(),
            jobId,
            startTime);
    }

    /// <summary>
    /// Gets a job builder with default options.
    /// </summary>
    /// <param name="jobCallbackName">The name of the JobCallback.</param>
    /// <param name="jobMetadata">The job metadata.</param>
    /// <param name="partitionId">The partition id of the job.</param>
    /// <param name="jobId">The id of the job.</param>
    /// <param name="startTime">The job start time.</param>
    /// <returns>A job builder</returns>
    protected static JobBuilder GetJobBuilderWithOptions(
        string jobCallbackName,
        StagedWorkerJobMetadata jobMetadata,
        string partitionId,
        string jobId = null,
        DateTime? startTime = null)
    {
        jobMetadata.TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        jobMetadata.SpanId = Activity.Current?.SpanId.ToString() ?? string.Empty;
        jobMetadata.RootTraceId = Activity.Current?.GetRootId() ?? string.Empty;

        // TODO: get subscription Id from the jobMetadata
        JobBuilder jobBuilder = JobBuilder.Create(partitionId, jobId ?? Guid.NewGuid().ToString())
            .WithCallback(jobCallbackName)
            .WithoutRepeatStrategy()
            .WithRetryStrategy(JobManager.DefaultRetryInterval, JobRecurrenceUnit.Second)
            .WithStartTime(startTime ?? DateTime.UtcNow)
            .WithMetadata(jobMetadata);

        return jobBuilder;
    }

    /// <summary>
    /// Queues a job if the job doesn't exist or if the existing job is faulted.
    /// </summary>
    /// <param name="operationType"></param>
    /// <param name="resourceType"></param>
    /// <param name="partitionId"></param>
    /// <param name="resourceId"></param>
    /// <param name="jobCallback"></param>
    /// <param name="jobMetadata"></param>
    /// <param name="startTime"></param>
    /// <param name="requeueIfSucceeded"></param>
    /// <returns>The job ID of the new/existing job</returns>
    protected async Task<string> CheckJobStatusAndQueueJobCallback(
        string operationType,
        string resourceType,
        string partitionId,
        Guid resourceId,
        string jobCallback,
        StagedWorkerJobMetadata jobMetadata,
        DateTime? startTime = null,
        bool requeueIfSucceeded = false)
    {
        string jobId = this.BuildJobId(operationType, resourceType, resourceId);

        return await this.CheckJobStatusAndQueueJobCallback(
            jobId,
            partitionId,
            jobCallback,
            jobMetadata,
            startTime,
            requeueIfSucceeded);
    }

    /// <summary>
    /// Queues a job if the job doesn't exist or if the existing job is faulted.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="partitionId"></param>
    /// <param name="jobCallback"></param>
    /// <param name="jobMetadata"></param>
    /// <param name="startTime"></param>
    /// <param name="requeueIfSucceeded"></param>
    /// <returns>The job ID of the new/existing job</returns>
    protected async Task<string> CheckJobStatusAndQueueJobCallback(
        string jobId,
        string partitionId,
        string jobCallback,
        StagedWorkerJobMetadata jobMetadata,
        DateTime? startTime = null,
        bool requeueIfSucceeded = false)
    {
        BackgroundJob existingJob = await this.GetJobAsync(partitionId, jobId);

        bool isJobInFaultedState = existingJob is { LastExecutionStatus: JobExecutionStatus.Faulted };
        bool isJobInSucceededState = requeueIfSucceeded ? existingJob is { LastExecutionStatus: JobExecutionStatus.Succeeded or JobExecutionStatus.Completed } : false;

        if (isJobInFaultedState || isJobInSucceededState)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"Deleting job {jobId} in status {existingJob?.LastExecutionStatus}");

            await this.DeleteJobAsync(partitionId, jobId);
            existingJob = null;
        }

        if (existingJob == null)
        {
            JobBuilder jobBuilder = JobManager.GetJobBuilderWithDefaultOptions(
                jobCallback,
                jobMetadata,
                partitionId,
                jobId);

            jobBuilder.WithStartTime(startTime ?? DateTime.UtcNow);

            await this.CreateJobAsync(jobBuilder);

            return jobBuilder.JobId;
        }

        return existingJob.JobId;
    }

    /// <inheritdoc />
    public async Task<string> DataEstateHealthFabricProvisioningJob(Guid accountId, Guid tenantId, Guid catalogId)
    {
        var jobMetadata = new DataEstateHealthFabricProvisioningJobMetadata
        {
            AccountId = accountId,
            TenantId = tenantId,
            CatalogId = catalogId,
            RequestHeaderContext = this.RequestHeaderContext,
            WorkerJobExecutionContext = this.WorkerJobExecutionContext
        };

        JobBuilder jobBuider = JobManager.GetJobBuilderWithDefaultOptions(
            nameof(DataEstateHealthFabricProvisioningJobCallback),
            jobMetadata,
            accountId.ToString());

        await this.CreateJobAsync(jobBuider);

        return jobBuider.JobId;
    }
}
