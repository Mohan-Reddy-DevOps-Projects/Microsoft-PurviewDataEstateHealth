// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// The contract for components capable of retrieving a single entity with an enum.
/// </summary>
public interface IRetrieveEntityOperations<TEntity, TEnum>
    where TEntity : class
    where TEnum : struct, Enum
{
    /// <summary>
    /// Retrieves a single entity.
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    /// <returns></returns>
    Task<TEntity> Get(TEnum enumType, CancellationToken cancellationToken);
}
