// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
///  Defines a contract for getting a single entity from persistence.
/// </summary>
public interface IGetSingleOperation<TEntity, in TEntityLocator>
{
    /// <summary>
    /// Retrieves a single entity.
    /// </summary>
    /// <param name="entityLocator">The id used to retrieve the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    Task<TEntity> GetSingle(TEntityLocator entityLocator, CancellationToken cancellationToken);
}
