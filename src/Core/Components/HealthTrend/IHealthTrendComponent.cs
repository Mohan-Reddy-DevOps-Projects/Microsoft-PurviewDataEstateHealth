// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing health trend collections.
/// </summary>
public interface IHealthTrendComponent : IRetrieveEntityOperations<IHealthTrendModel, TrendKind>,
    INavigable<Guid, IHealthTrendComponent>, IComponent<IHealthTrendContext>
{
}
