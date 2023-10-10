// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
/// The contract for components capable of retrieving entity collections.
/// </summary>
public interface IRetrieveEntityCollectionOperations<TEntity>
{
    /// <summary>
    /// Retrieves an entity collection.
    /// </summary>
    /// <param name="skipToken">Continuation Token to get next set of results</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that resolves to the entity collection.</returns>
    Task<IBatchResults<TEntity>> Get(
        CancellationToken cancellationToken,
        string skipToken = null);
}
