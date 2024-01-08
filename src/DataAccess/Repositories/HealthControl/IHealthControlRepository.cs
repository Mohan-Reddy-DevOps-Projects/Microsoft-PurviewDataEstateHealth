// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health control repository interface.
/// </summary>
public interface IHealthControlRepository :
    IGetMultipleOperation<HealthControlModel, HealthControlsKey>,
    IGetMultipleOperation<HealthControlModel, HealthControlKey>
{
}
