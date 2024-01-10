// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
/// Health score repository interface
/// </summary>
public interface IHealthScoreRepository :
    IGetMultipleOperation<IHealthScoreModel<HealthScoreProperties>, HealthScoreKey>,
    ILocationBased<IHealthScoreRepository>
{
    /// <summary>
    /// Gets the <see cref="IHealthScoreModel{TProperties}"/> associated with the specified key or the default value.
    /// </summary>
    /// <param name="healthScoreKey">The key of the value to get.</param>
    /// <returns>The <see cref="IHealthScoreModel{TProperties}"/> if found; otherwise null.</returns>
    public Task<IHealthScoreModel<HealthScoreProperties>> GetSingleOrDefault(HealthScoreKey healthScoreKey);
}
