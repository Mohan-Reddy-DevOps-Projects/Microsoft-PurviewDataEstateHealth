// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health score repository interface
/// </summary>
public interface IHealthScoreRepository : IGetMultipleOperation<IHealthScoreModel<HealthScoreProperties>, HealthScoreKey>,
    ILocationBased<IHealthScoreRepository>
{
}
