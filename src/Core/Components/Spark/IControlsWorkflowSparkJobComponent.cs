// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Controls Workflow Spark job component.
/// </summary>
public interface IControlsWorkflowSparkJobComponent
{
    /// <summary>
    /// Submit a controls workflow spark job.
    /// </summary>
    /// <param name="accountServiceModel">Account service model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="sparkPoolId">Spark pool ID</param>
    /// <param name="isDEHDataCleanup">Flag for DEH data cleanup</param>
    /// <returns>Spark pool job model</returns>
    Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId, string sparkPoolId, bool isDEHDataCleanup);

    /// <summary>
    /// Get details for a spark job.
    /// </summary>
    /// <param name="accountServiceModel">Account service model</param>
    /// <param name="batchId">Batch ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Spark batch job details</returns>
    Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Get details for a spark job.
    /// </summary>
    /// <param name="jobInfo">Job info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Spark batch job details</returns>
    Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken);
} 