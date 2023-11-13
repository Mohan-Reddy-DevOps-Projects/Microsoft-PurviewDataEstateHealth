// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health action repository interface
/// </summary>
public interface IHealthActionRepository : IGetMultipleOperation<IHealthActionModel, HealthActionKey>,
    ILocationBased<IHealthActionRepository>
{
}
