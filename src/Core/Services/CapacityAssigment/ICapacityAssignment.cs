// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Capacity assignment service
/// </summary>
public interface ICapacityAssignment
{
    /// <summary>
    /// Assign a workspace to the most appropriate capacity based on:
    /// 1) Capacity location
    /// 2) SKU - Free tier vs enterprise
    /// 3) Capacity utilization (TBD)
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AssignWorkspace(Guid profileId, Guid workspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the capacity designated for the given account.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Capacity> GetCapacity(Guid profileId, CancellationToken cancellationToken);
}
