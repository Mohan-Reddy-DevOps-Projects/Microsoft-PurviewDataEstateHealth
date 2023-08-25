// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of creating and updating entities.
/// </summary>
public interface IEntityCreateOrUpdateOperation<TPayload, TEntity>
{
    /// <summary>
    /// Create or update a new entity.
    /// </summary>
    /// <param name="payload">The information needed to create the entity.</param>
    /// <returns>A task that resolves to the newly created entity.</returns>
    Task<TEntity> CreateOrUpdate(TPayload payload);
}
