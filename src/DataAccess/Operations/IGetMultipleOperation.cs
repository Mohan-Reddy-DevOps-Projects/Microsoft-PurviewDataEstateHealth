// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
///  Defines a contract for getting a multiple entitities from persistence.
/// </summary>
public interface IGetMultipleOperation<TEntity, in TCriteria>
{
    /// <summary>
    /// Retrieves multiple entities.
    /// </summary>
    /// <param name="criteria"> The type of search criteria to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continuationToken">The continuation token.</param>
    Task<IBatchResults<TEntity>> GetMultiple(TCriteria criteria, CancellationToken cancellationToken, string continuationToken = null);
}
