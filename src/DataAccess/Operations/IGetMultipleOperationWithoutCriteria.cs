// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
///  Defines a contract for getting a multiple entitities from persistence.
/// </summary>
public interface IGetMultipleOperationWithoutCriteria<TEntity>
{
    /// <summary>
    /// Retrieves multiple entities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continuationToken">The continuation token.</param>
    Task<IBatchResults<TEntity>> GetMultiple(CancellationToken cancellationToken, string continuationToken = null);
}
