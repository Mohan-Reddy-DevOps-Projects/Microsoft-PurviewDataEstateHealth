// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// The contract for components capable of retrieving a single entity.
/// </summary>
public interface IRetrieveEntityByIdOperation<TId, TEntity>
{
    /// <summary>
    /// Retrieves a single entity.
    /// </summary>
    /// <param name="id">The id used to retrieve the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    Task<TEntity> Get(TId id, CancellationToken cancellationToken);
}
