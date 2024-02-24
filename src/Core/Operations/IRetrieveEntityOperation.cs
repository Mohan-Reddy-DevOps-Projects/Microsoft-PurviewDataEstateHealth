// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of retrieving a single entity.
/// </summary>
public interface IRetrieveEntityOperation<TEntity>
{
    /// <summary>
    /// Retrieves a single entity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    /// <returns></returns>
    Task<TEntity> Get(CancellationToken cancellationToken);
}
