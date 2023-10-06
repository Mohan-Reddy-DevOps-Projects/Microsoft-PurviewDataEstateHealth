// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Health report Component contract.
/// </summary>
public interface IHealthReportComponent : IRetrieveEntityOperation<IHealthReportModel<HealthReportProperties>>,
    IComponent<IHealthReportContext>
{

}
