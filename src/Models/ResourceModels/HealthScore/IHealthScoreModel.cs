// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the health score model.
/// </summary>
public interface IHealthScoreModel<out TProperties> : IPersistedResource<TProperties>
    where TProperties : HealthScoreProperties, new()
{
}
