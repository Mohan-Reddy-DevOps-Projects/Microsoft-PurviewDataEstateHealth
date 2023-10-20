// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the health control model.
/// </summary>
public interface IHealthControlModel<out TProperties> : IPersistedResource<TProperties>
    where TProperties : HealthControlProperties
{
}
