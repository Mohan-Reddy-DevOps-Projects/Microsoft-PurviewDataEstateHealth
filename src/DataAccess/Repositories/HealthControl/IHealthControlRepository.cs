// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health control repository interface.
/// </summary>
public interface IHealthControlRepository :
    IGetMultipleOperation<IHealthControlModel<HealthControlProperties>, HealthControlsKey>,
    ILocationBased<IHealthControlRepository>,
    IGetMultipleOperation<IHealthControlModel<HealthControlProperties>, HealthControlKey>
{
}
