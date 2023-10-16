// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing health score collections.
/// </summary>
public interface IHealthScoreCollectionComponent :
    IRetrieveEntityCollectionOperations<IHealthScoreModel<HealthScoreProperties>>, INavigable<Guid, IHealthScoreCollectionComponent>,
    IComponent<IHealthScoreListContext>
{
}
