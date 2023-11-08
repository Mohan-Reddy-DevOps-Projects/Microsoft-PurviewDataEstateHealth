// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using global::Azure.ResourceManager.Synapse;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

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
}
