// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

/// <summary>
/// Spark job component.
/// </summary>
public interface ICatalogSparkJobComponent
{
    /// <summary>
    /// Submit a spark job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>    
    Task<string> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId);


    /// <summary>
    /// Get details for a spark job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken);
}
