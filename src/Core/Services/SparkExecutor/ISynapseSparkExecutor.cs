// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.ResourceManager.Synapse;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the operations to be performed on the Synapse Spark pool.
/// </summary>
public interface ISynapseSparkExecutor
{
    /// <summary>
    /// Creates the spark pool.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SynapseBigDataPoolInfoData> CreateOrUpdateSparkPool(string sparkPoolName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the spark pool.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SynapseBigDataPoolInfoData> GetSparkPool(string sparkPoolName, CancellationToken cancellationToken);

    /// <summary>
    /// List the spark pools.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<SynapseBigDataPoolInfoData>> ListSparkPools(CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the spark pool.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteSparkPool(string sparkPoolName, CancellationToken cancellationToken);

    /// <summary>
    /// Whether the spark pool exists or not.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SparkPoolExists(string sparkPoolName, CancellationToken cancellationToken);

    /// <summary>
    /// Submits the job.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="sparkJobRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> SubmitJob(string sparkPoolName, SparkJobRequest sparkJobRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the job.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelJob(string sparkPoolName, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the job details.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkBatchJob> GetJob(string sparkPoolName, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the jobs.
    /// </summary>
    /// <param name="sparkPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<SparkBatchJob>> ListJobs(string sparkPoolName, CancellationToken cancellationToken);
}
