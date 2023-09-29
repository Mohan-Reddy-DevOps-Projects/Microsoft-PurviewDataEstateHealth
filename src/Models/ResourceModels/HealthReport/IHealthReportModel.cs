// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the health report model.
/// </summary>
public interface IHealthReportModel<out TProperties> : IPersistedResource<TProperties>
    where TProperties : HealthReportProperties
{
  
}
