// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Spark job manager interface.
/// </summary>
public interface ISparkJobManager
{
    /// <summary>
    /// Submits the job.
    /// </summary>
    /// <param name="sparkJobRequest"></param>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="existingPoolResourceId"></param>
    /// <param name="retryCount"></param>
    /// <param name="poolConfigAction">Optional action to customize the spark pool configuration</param>
    /// <returns></returns>
    Task<SparkPoolJobModel> SubmitJob(SparkJobRequest sparkJobRequest, AccountServiceModel accountServiceModel, CancellationToken cancellationToken, ResourceIdentifier existingPoolResourceId = null, int retryCount = 5, Action<SynapseBigDataPoolInfoData> poolConfigAction = null);

    /// <summary>
    /// Cancels the job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the job.
    /// </summary>
    /// <param name="jobModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelJob(SparkPoolJobModel jobModel, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the job.
    /// </summary>
    /// <param name="jobModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkBatchJob> GetJob(SparkPoolJobModel jobModel, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates the spark pool.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="configAction">Optional action to customize the spark pool configuration</param>
    /// <returns></returns>
    Task<SparkPoolModel> CreateOrUpdateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, Action<SynapseBigDataPoolInfoData> configAction = null);

    /// <summary>
    /// Gets the spark pool.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkPoolModel> GetSparkPool(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the spark pool.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteSparkPoolRecord(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the spark pool.
    /// </summary>
    /// <param name="poolResourceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteSparkPool(string poolResourceId, CancellationToken cancellationToken);
}
