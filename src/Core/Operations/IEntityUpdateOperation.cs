// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of updating entities.
/// </summary>
public interface IEntityUpdateOperation<TPayload, TEntity>
{
    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="payload">The information needed to update  the entity.</param>
    /// <returns>A task that resolves to the updated entity.</returns>
    Task<TEntity> Update(TPayload payload);
}
