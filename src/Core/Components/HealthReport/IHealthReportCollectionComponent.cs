// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing health report collections.
/// </summary>
public interface IHealthReportCollectionComponent :
    IRetrieveEntityCollectionOperation<IHealthReportModel<HealthReportProperties>, HealthReportKind>,
    IComponent<IHealthReportListContext>, INavigable<Guid, IHealthReportComponent>
{
}
