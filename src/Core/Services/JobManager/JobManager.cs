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
    private const long DefaultRetryInterval = 60;

    /// <summary>
    /// Job ID format
    /// </summary>
    private static readonly string jobIdFormat = $"{{0}}-{{1}}-{{2}}";

    /// <summary>
    /// Job management client for managing jobs
    /// </summary>
    private Lazy<Task<JobManagementClient>> JobManagementClient;

    /// <summary>
    /// Request header context accessor
    /// </summary>
    private IRequestContextAccessor requestContextAccessor;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <summary>
    /// The Execution Context for the Job Manager.
    /// </summary>
    private readonly WorkerJobExecutionContext WorkerJobExecutionContext;

    private const int JobsMinStartTime = 7; //Minutes
    private const int JobsMaxStartTime = 1441; //Minutes
    private const int SparkJobsRetryStrategyTime = 15; //Minutes

    static readonly string CatalogSparkJobPartitionAffix = "-CATALOG-SPARK-JOBS";
    static readonly string CatalogSparkJobIdAffix = "-CATALOG-SPARK-JOB";

    static readonly string DataQualitySparkJobPartitionAffix = "-SPARK-JOBS";
    static readonly string DataQualitySparkJobIdAffix = "-DATAQUALITY-SPARK-JOB";

    static readonly string ActionCleanUpJobPartitionAffix = "-ACTION-CLEAN-UP-JOBS";
    static readonly string ActionCleanUpJobIdAffix = "-ACTION-CLEAN-UP-JOB";

    private EnvironmentConfiguration environmentConfiguration;
    private static readonly Random RandomGenerator = new();

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
    public async Task RunPBIRefreshJob(AccountServiceModel accountModel)
    {
        string jobPartition = $"PBI-REFRESH-CALLBACK-IMMEDIEATE";
        string jobId = $"{accountModel.Id}-PBI-REFRESH-CALLBACK-IMMEDIEATE";
        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        StartPBIRefreshMetadata jobMetadata = new()
        {
            RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
            WorkerJobExecutionContext = WorkerJobExecutionContext.None,
            Account = accountModel,
            RefreshLookups = new List<RefreshLookup>()
        };

        var jobOptions = new BackgroundJobOptions()
        {
            CallbackName = nameof(PBIRefreshCallback),
            JobPartition = jobPartition,
            JobId = jobId,
            StartTime = DateTime.UtcNow,
            RepeatInterval = TimeSpan.FromMinutes(5),
        };
        await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);

        this.dataEstateHealthRequestLogger.LogInformation($"PBIRefresh job created. Partition key: {jobPartition}.");
    }

    /// <inheritdoc />
    public async Task StartDimensionModelRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel accountServiceModel)
    {
        string accountId = accountServiceModel.Id;
        string jobPartition = $"DIMENSION-SPARK-JOBS";
        string jobId = $"{accountId}-DIMENSION-SPARK-JOBS";

        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
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
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(DimensionModelSparkJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }


    /// <inheritdoc />
    public async Task StartFabricelRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel accountServiceModel)
    {

        string accountId = accountServiceModel.Id;

        string jobPartition = $"FABRIC-SPARK-JOBS";
        string jobId = $"{accountId}-FABRIC-SPARK-JOBS";

        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }


        var repeatInterval = TimeSpan.FromDays(1); // this.environmentConfiguration.IsDevelopmentOrDogfoodEnvironment() ? 5 : 15);

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

            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(FabricSparkJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
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
                DataQualityEventsProcessed = false,
                ProcessingStoresCache = new Dictionary<Guid, string>(),
            };
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(PartnerEventsConsumerJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = TimeSpan.FromMinutes(5),
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
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
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(MDQFailedJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = TimeSpan.FromHours(6),
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }

    /// <inheritdoc />
    public async Task ProvisionBackgroundJobCleanupJob()
    {
        string jobPartition = "BACKGROUND-JOB-CLEANUP";
        string jobId = "BACKGROUND-JOB-CLEANUP-WORKER";
        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new StagedWorkerJobMetadata()
            {
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
            };
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(BackgroundJobCleanupCallbackJob),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = TimeSpan.FromHours(1),
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }

    public async Task ProvisionActionsCleanupJob(AccountServiceModel accountServiceModel)
    {
        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{ActionCleanUpJobPartitionAffix}";
        string jobId = $"{accountId}{ActionCleanUpJobIdAffix}";

        BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);

        if (job != null && job.State == JobState.Faulted)
        {
            await this.DeleteJobAsync(jobPartition, jobId);
            job = null;
        }

        if (job == null)
        {
            var jobMetadata = new ActionCleanUpJobMetadata
            {
                WorkerJobExecutionContext = WorkerJobExecutionContext.None,
                RequestContext = new CallbackRequestContext(this.requestContextAccessor.GetRequestContext()),
                AccountServiceModel = accountServiceModel,
                ActionCleanUpCompleted = false,
            };

            var repeatInterval = TimeSpan.FromDays(1);
            int randomMins = RandomGenerator.Next(JobsMinStartTime, JobsMaxStartTime);
            var startTime = DateTime.UtcNow.AddMinutes(randomMins);
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(DHActionJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = repeatInterval,
                StartTime = startTime
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }

    public async Task DeprovisionActionsCleanupJob(AccountServiceModel accountServiceModel)
    {
        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{ActionCleanUpJobPartitionAffix}";
        string jobId = $"{accountId}{ActionCleanUpJobIdAffix}";
        await this.DeleteJobAsync(jobPartition, jobId);
    }


    /// <inheritdoc />
    public async Task ProvisionCatalogSparkJob(AccountServiceModel accountServiceModel)
    {
        var catalogRepeatStrategy = this.environmentConfiguration.IsDevelopmentOrDogfoodEnvironment() ?
        TimeSpan.FromHours(1) : TimeSpan.FromHours(12);

        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{CatalogSparkJobPartitionAffix}";
        string jobId = $"{accountId}{CatalogSparkJobIdAffix}";

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
            int randomMins = this.environmentConfiguration.IsDevelopmentOrDogfoodEnvironment() ? 3 : RandomGenerator.Next(JobsMinStartTime, JobsMaxStartTime);

            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(CatalogSparkJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = catalogRepeatStrategy,
                StartTime = DateTime.UtcNow.AddMinutes(randomMins),
                RetryStrategy = TimeSpan.FromMinutes(SparkJobsRetryStrategyTime)
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }

    public async Task DeprovisionCatalogSparkJob(AccountServiceModel accountServiceModel)
    {
        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{CatalogSparkJobPartitionAffix}";
        string jobId = $"{accountId}{CatalogSparkJobIdAffix}";
        await this.DeleteJobAsync(jobPartition, jobId);
    }

    /// <inheritdoc />
    public async Task ProvisionDataQualitySparkJob(AccountServiceModel accountServiceModel)
    {
        const int dqRepeatStrategyTime = 1; // Hours

        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{DataQualitySparkJobPartitionAffix}";
        string jobId = $"{accountId}{DataQualitySparkJobIdAffix}";

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

            int randomMins = RandomGenerator.Next(JobsMinStartTime, JobsMaxStartTime);
            var jobOptions = new BackgroundJobOptions()
            {
                CallbackName = nameof(DataQualitySparkJobCallback),
                JobPartition = jobPartition,
                JobId = jobId,
                RepeatInterval = TimeSpan.FromHours(dqRepeatStrategyTime),
                StartTime = DateTime.UtcNow.AddMinutes(randomMins),
                RetryStrategy = TimeSpan.FromMinutes(SparkJobsRetryStrategyTime)
            };
            await this.CreateBackgroundJobAsync(jobMetadata, jobOptions);
        }
    }
    public async Task DeprovisionDataQualitySparkJob(AccountServiceModel accountServiceModel)
    {
        string accountId = accountServiceModel.Id;
        string jobPartition = $"{accountId}{DataQualitySparkJobPartitionAffix}";
        string jobId = $"{accountId}{DataQualitySparkJobIdAffix}";
        await this.DeleteJobAsync(jobPartition, jobId);
    }

    public async Task TriggerBackgroundJobAsync(string jobPartition, string jobId, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Trigger background job. Job partition: {jobPartition}. Job ID: {jobId}."))
        {
            try
            {
                JobManagementClient jobClient = await this.JobManagementClient.Value;

                await PollyRetryPolicies
                    .GetNonHttpClientTransientRetryPolicy(
                        LoggerRetryActionFactory.CreateWorkerRetryAction(this.dataEstateHealthRequestLogger, nameof(JobManager)))
                    .ExecuteAsync(() => jobClient.RunJob(jobPartition, jobId, DateTime.UtcNow));
                this.dataEstateHealthRequestLogger.LogInformation($"Succeeded to trigger background job. {jobPartition} {jobId}");
            }
            catch (Exception exception)
            {
                this.dataEstateHealthRequestLogger.LogError($"Fail to trigger background job. {jobPartition} {jobId}", exception);
                throw;
            }
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
    private async Task CreateJobAsync(JobBuilder jobBuilder, [CallerMemberName] string operationName = "")
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

    private async Task CreateBackgroundJobAsync<TMetadata>(TMetadata metadata, BackgroundJobOptions options)
        where TMetadata : StagedWorkerJobMetadata
    {
        this.dataEstateHealthRequestLogger.LogInformation($"Create background job. {options}");

        this.UpdateDerivedMetadataProperties(metadata);

        metadata.TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        metadata.SpanId = Activity.Current?.SpanId.ToString() ?? string.Empty;
        metadata.RootTraceId = Activity.Current?.GetRootId() ?? string.Empty;

        var jobPartition = options.JobPartition;
        var jobId = options.JobId;

        if (jobId != null)
        {
            BackgroundJob job = await this.GetJobAsync(jobPartition, jobId);
            if (job != null)
            {
                this.dataEstateHealthRequestLogger.LogInformation($"Background job existed. Job status: {job.LastExecutionStatus}");
                await this.DeleteJobAsync(jobPartition, jobId).ConfigureAwait(false);
                this.dataEstateHealthRequestLogger.LogInformation($"Existed background job successfully deleted. {jobId}");
            }
        }

        JobBuilder jobBuilder = JobBuilder.Create(jobPartition, jobId ?? Guid.NewGuid().ToString())
            .WithCallback(options.CallbackName)
            .WithMetadata(metadata)
            .WithStartTime(options.StartTime ?? DateTime.UtcNow.AddMinutes(1))
            .WithRetryStrategy(options.RetryStrategy ?? TimeSpan.FromSeconds(JobManager.DefaultRetryInterval))
            .WithoutEndTime()
            .WithRetention(TimeSpan.FromDays(7))
            .WithFlags(JobFlags.DeleteJobIfCompleted);

        if (options.RepeatInterval.HasValue)
        {
            jobBuilder = jobBuilder.WithRepeatStrategy(options.RepeatInterval.Value);
        }
        else
        {
            jobBuilder = jobBuilder.WithoutRepeatStrategy();
        }

        await this.CreateJobAsync(jobBuilder);
    }
}

class BackgroundJobOptions
{
    public string CallbackName { get; set; }

    public string JobPartition { get; set; }

    public string JobId { get; set; }

    public TimeSpan? RetryStrategy { get; set; }

    public TimeSpan? RepeatInterval { get; set; }

    public DateTime? StartTime { get; set; }

    public override string ToString()
    {
        return $"{this.JobPartition}, {this.JobId}, {this.CallbackName}, {this.RepeatInterval?.ToString()}, {this.StartTime?.ToString()}";
    }
}
