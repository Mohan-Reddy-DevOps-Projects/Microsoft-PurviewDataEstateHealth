// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing health control collections.
/// </summary>
public interface IHealthControlCollectionComponent :
    IRetrieveEntityCollectionOperations<IHealthControlModel<HealthControlProperties>>,
    IComponent<IHealthControlListContext>
{
}
