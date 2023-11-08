// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

/// <summary>
/// Spark job manager interface.
/// </summary>
public interface ISparkJobManager
{
    /// <summary>
    /// Submits the job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="sparkJobRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SubmitJob(AccountServiceModel accountServiceModel, SparkJobRequest sparkJobRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="batchId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates the spark pool.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkPoolModel> CreateOrUpdateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the spark pool.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SparkPoolModel> GetSparkPool(Guid accountId, CancellationToken cancellationToken);
}
