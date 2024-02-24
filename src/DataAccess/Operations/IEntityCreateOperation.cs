// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of creating entities.
/// </summary>
public interface IEntityCreateOperation<TPayload, TEntity>
{
    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="payload">The information needed to create the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the newly created entity.</returns>
    Task<TEntity> Create(TPayload payload, CancellationToken cancellationToken);
}
