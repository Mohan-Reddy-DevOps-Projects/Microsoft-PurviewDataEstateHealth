// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Job Manager Interface that manages background jobs
/// </summary>
public interface IJobManager
{
    /// <summary>
    /// Builds the job ID for a background job.
    /// </summary>
    /// <param name="operationType"></param>
    /// <param name="resourceType"></param>
    /// <param name="resourceId"></param>
    /// <returns>The job ID</returns>
    string BuildJobId(string operationType, string resourceType, Guid resourceId);

    /// <summary>
    /// Deletes a background job.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="jobId"></param>
    Task DeleteJobAsync(string accountId, string jobId);

    /// <summary>
    /// Get a background job.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="jobId"></param>
    /// <returns>The background job</returns>
    Task<BackgroundJob> GetJobAsync(string accountId, string jobId);

    /// <summary>
    /// Gets a list of jobs by partition.
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns>The list of jobs</returns>
    Task<BackgroundJob[]> GetJobsAsync(string accountId);

    /// <summary>
    /// Gets an array of jobs by partition and jobIds.
    /// </summary>
    /// <param name="accountId">The accountId.</param>
    /// <param name="jobIds">The ids of the jobs.</param>
    /// <returns>The array of <see cref="BackgroundJob"/>.</returns>
    Task<BackgroundJob[]> GetJobsAsync(string accountId, List<string> jobIds);

    /// <summary>
    ///  Gets a list of jobs by partition and checks the status of the given background jobs
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="jobIds"></param>
    /// <returns>The list of jobs</returns>
    Task<AggregateJobExecutionStatus> GetJobsAndExecutionStatusAsync(string accountId, List<string> jobIds);

    /// <summary>
    /// Checks the execution status of the given backgrounds jobs.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="jobIds"></param>
    /// <returns>A single status representing the statuses of all the given jobs</returns>
    Task<JobExecutionStatus> GetJobsStatusAsync(string accountId, List<string> jobIds);

    /// <summary>
    /// Returns a <see cref="JobExecutionStatus"/> for the provided <see cref="BackgroundJob"/> that consolidates
    /// unused <see cref="JobState"/>s.
    /// </summary>
    /// <param name="backgroundJob">The <see cref="BackgroundJob"/> to get the JobExecutionStatus for.</param>
    /// <returns>A <see cref="JobExecutionStatus"/> for the <see cref="BackgroundJob"/>.</returns>
    JobExecutionStatus ReduceJobExecutionStatus(BackgroundJob backgroundJob);

    /// <summary>
    /// Returns a <see cref="JobExecutionStatus"/> representing the consolidated status for those provided in array.
    /// </summary>
    /// <param name="jobStatuses">An array of <see cref="JobExecutionStatus"/>.</param>
    /// <returns>A <see cref="JobExecutionStatus"/> for the array.</returns>
    JobExecutionStatus ReduceJobExecutionStatuses(JobExecutionStatus[] jobStatuses);

    /// <summary>
    /// Returns true if and only the specified job's metadata contains an exception
    /// with the given error code.
    /// </summary>
    /// <param name="partitionId"></param>
    /// <param name="jobId"></param>
    /// <param name="errorCode"></param>
    Task<bool> JobMetadataContainsErrorCodeAsync(
        string partitionId,
        string jobId,
        ErrorCode errorCode);

    /// <summary>
    /// Provisions event processing job.
    /// </summary>
    /// <returns></returns>
    Task ProvisionEventProcessorJob();

    /// <summary>
    /// Provisions MDQ failed job.
    /// </summary>
    /// <returns></returns>
    Task ProvisionMDQFailedJob();

    /// <summary>
    /// Start PBI refresh job
    /// </summary>
    /// <returns></returns>
    Task StartPBIRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel accountModel);

    /// <summary>
    /// Provisions catalog SPARK job per account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <returns></returns>
    Task ProvisionCatalogSparkJob(AccountServiceModel accountServiceModel);

    /// <summary>
    /// Provisions data quality SPARK job per account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <returns></returns>
    Task ProvisionDataQualitySparkJob(AccountServiceModel accountServiceModel);
}
