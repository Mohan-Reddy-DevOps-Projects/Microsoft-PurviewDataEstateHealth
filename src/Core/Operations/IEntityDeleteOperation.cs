// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Purview.DataGovernance.Reporting.Common;

/// <summary>
/// The contract for components capable of deleting entities.
/// </summary>
public interface IEntityDeleteOperation
{
    /// <summary>
    /// Deletes an existing entity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Deletion status and job id if deletion was performed asynchronously</returns>
    Task<DeletionResult> Delete(CancellationToken cancellationToken);
}
