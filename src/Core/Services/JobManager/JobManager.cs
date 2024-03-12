// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

/// <inheritdoc />
public class JobManager : IJobManager
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
    /// Request header context accessor
    /// </summary>
    protected IRequestContextAccessor requestContextAccessor;

    /// <summary>
    /// Logger
    /// </summary>
    protected readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <summary>
    /// The Execution Context for the Job Manager.
    /// </summary>
    protected readonly WorkerJobExecutionContext WorkerJobExecutionContext;

    private const int SparkJobsStartTime = 7; //Minutes
    private const int SparkJobsRetryStrategyTime = 15; //Minutes

    private EnvironmentConfiguration environmentConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobManager" /> class.
    /// </summary>
    public JobManager(
        IRequestContextAccessor requestContextAccessor,
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IJobManagementStorageAccountBuilder jobStorageAccountBuilder,
        WorkerJobExecutionContext workerJobExecutionContext = WorkerJobExecutionContext.None)
    {
        this.JobManagementClient = new Lazy<Task<JobManagementClient>>(async () =>
        {
            var dataConsistencyOptions = new StorageConsistencyOptions
            {
                ShardingEnabled = false,
                ReplicationEnabled = false
            };

            WindowsAzure.Storage.CloudStorageAccount cloudStorageAccount = await jobStorageAccountBuilder.Build();

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

        this.requestContextAccessor = requestContextAccessor;
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
        this.WorkerJobExecutionContext = workerJobExecutionContext;
        this.environmentConfiguration = environmentConfiguration.Value;
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

    /// <inheritdoc />
    public async Task StartPBIRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel accountModel)
    {
        string jobPartition = "PBI-REFRESH-CALLBACK";

        StartPBIRefreshMetadata jobMetadata = new()
        {
            RequestContext = metadata.RequestContext,
            WorkerJobExecutionContext = WorkerJobExecutionContext.None,
            Account = accountModel,
            RefreshLookups = new List<RefreshLookup>()
        };

        await this.CreateOneTimeJob(jobMetadata, nameof(PBIRefreshCallback), jobPartition);
    }

    /// <inheritdoc />
    public async Task ProvisionEventProcessorJob()
    {
        string jobPartition = "PARTNER-EVENT-CONSUMERS";
        string jobId = "DATA-GOVERNANCE-PARTNER-EVENTS-CONSUMER";
        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new PartnerEventsConsumerJobMetadata
            {
                WorkerJobExecutionContext = WorkerJobExecutionContext.None,
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
                DataAccessEventsProcessed = false,
                DataCatalogEventsProcessed = false,
                DataQualityEventsProcessed = false,
                ProcessingStoresCache = new Dictionary<Guid, string>(),
            };

            await this.CreateOneTimeJob(jobMetadata, nameof(PartnerEventsConsumerJobCallback), jobPartition, jobId);
        }
    }

    /// <inheritdoc />
    public async Task ProvisionMDQFailedJob()
    {
        string jobPartition = "MDQ-FAILED-JOB-CHECKER";
        string jobId = "MDQ-FAILED-JOB-WORKER";
        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new MDQFailedJobMetadata
            {
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
                MDQFailedJobProcessed = false,
            };
            await this.CreateOneTimeJob(jobMetadata, nameof(MDQFailedJobCallback), jobPartition, jobId);
        }
    }

    private async Task CreateOneTimeJob<TMetadata>(TMetadata metadata, string jobCallbackName, string jobPartition, string jobId = null)
        where TMetadata : StagedWorkerJobMetadata
    {
        this.UpdateDerivedMetadataProperties(metadata);

        var repeatInterval = TimeSpan.FromMinutes(5);

        JobBuilder jobBuilder = GetJobBuilderWithDefaultOptions(
                    jobCallbackName,
                    metadata,
                    jobPartition,
                    jobId)
                .WithStartTime(DateTime.UtcNow.AddMinutes(1))
                .WithRepeatStrategy(repeatInterval)
                .WithRetryStrategy(TimeSpan.FromMinutes(5))
                .WithoutEndTime();

        await this.CreateJobAsync(jobBuilder);
    }

    /// <inheritdoc />
    public async Task ProvisionCatalogSparkJob(AccountServiceModel accountServiceModel)
    {
        const int catalogRepeatStrategyTime = 1; // Days

        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}-SPARK-JOBS";
        string jobId = $"{accountId}-CATALOG-SPARK-JOB";

        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null && job.State == JobState.Faulted)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new SparkJobMetadata
            {
                WorkerJobExecutionContext = WorkerJobExecutionContext.None,
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
                AccountServiceModel = accountServiceModel,
                SparkJobBatchId = string.Empty,
                IsCompleted = false
            };
            this.UpdateDerivedMetadataProperties(jobMetadata);

            JobBuilder jobBuilder = GetJobBuilderWithDefaultOptions(
                    nameof(CatalogSparkJobCallback),
                    jobMetadata,
                    jobPartition,
                    jobId)
                .WithStartTime(DateTime.UtcNow.AddMinutes(SparkJobsStartTime))
                .WithRepeatStrategy(TimeSpan.FromDays(catalogRepeatStrategyTime))
                .WithRetryStrategy(TimeSpan.FromMinutes(SparkJobsRetryStrategyTime))
                .WithoutEndTime();

            await this.CreateJobAsync(jobBuilder);
        }
    }

    /// <inheritdoc />
    public async Task ProvisionDataQualitySparkJob(AccountServiceModel accountServiceModel)
    {
        const int dqRepeatStrategyTime = 1; // Hours

        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}-SPARK-JOBS";
        string jobId = $"{accountId}-DATAQUALITY-SPARK-JOB";

        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null && job.State == JobState.Faulted)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new SparkJobMetadata
            {
                WorkerJobExecutionContext = WorkerJobExecutionContext.None,
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
                AccountServiceModel = accountServiceModel,
                SparkJobBatchId = string.Empty,
                IsCompleted = false
            };

            this.UpdateDerivedMetadataProperties(jobMetadata);

            JobBuilder jobBuilder = GetJobBuilderWithDefaultOptions(
                    nameof(DataQualitySparkJobCallback),
                    jobMetadata,
                    jobPartition,
                    jobId)
                .WithStartTime(DateTime.UtcNow.AddMinutes(SparkJobsStartTime))
                .WithRepeatStrategy(TimeSpan.FromHours(dqRepeatStrategyTime))
                .WithRetryStrategy(TimeSpan.FromMinutes(SparkJobsRetryStrategyTime))
                .WithoutEndTime();

            await this.CreateJobAsync(jobBuilder);
        }
    }

    private void UpdateDerivedMetadataProperties<TMetadata>(TMetadata metadata) where TMetadata : JobMetadataBase
    {
        metadata.TenantId = metadata.RequestContext.TenantId.ToString();
        if (RequestCorrelationContext.Current.CorrelationId == null)
        {
            // Set correlation-id if not set in the payload.
            if (string.IsNullOrEmpty(metadata.RequestContext.CorrelationId))
            {
                Guid correlationId = Guid.NewGuid();
                metadata.RequestContext.CorrelationId = correlationId.ToString();
            }

            RequestCorrelationContext requestCorrelationContext = new RequestCorrelationContext()
            {
                CorrelationId = metadata.RequestContext.CorrelationId,
            };

            RequestCorrelationContext.Current.Initialize(requestCorrelationContext);
        }
        else
        {
            metadata.RequestContext.CorrelationId = RequestCorrelationContext.Current.CorrelationId;
        }
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
}
