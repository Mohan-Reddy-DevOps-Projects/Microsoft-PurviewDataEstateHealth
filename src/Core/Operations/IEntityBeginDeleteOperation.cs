// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Purview.DataGovernance.Reporting.Common;

/// <summary>
/// The contract for components capable of kicking off the delete resource job
/// </summary>
public interface IEntityBeginDeleteOperation
{
    /// <summary>
    /// Starts deletion of an existing entity.
    /// </summary>
    /// <returns>Deletion status and job id if deletion was performed asynchronously</returns>
    Task<DeletionResult> BeginDelete();
}
