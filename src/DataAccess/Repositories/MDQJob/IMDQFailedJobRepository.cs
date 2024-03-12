// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Storage account repository interface
/// </summary>
public interface IMDQFailedJobRepository
{
    /// <summary>
    /// Creates job
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MDQJobModel> Create(MDQJobModel model, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes job
    /// </summary>
    /// <param name="rowKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Delete(Guid rowKey, CancellationToken cancellationToken);

    /// <summary>
    /// Gets existing job list
    /// </summary>
    /// <param name="maxPerPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// 
    Task<List<MDQJobModel>> GetBulk(int maxPerPage, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single entity.
    /// </summary>
    /// <param name="rowKey">The id used to retrieve the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    Task<MDQJobModel> GetSingle(Guid rowKey, CancellationToken cancellationToken);

    /// <summary>
    /// Updates job
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Update(MDQJobModel model, CancellationToken cancellationToken);
}
