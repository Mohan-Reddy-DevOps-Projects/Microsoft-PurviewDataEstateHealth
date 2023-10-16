// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing health action collections.
/// </summary>
public interface IHealthActionCollectionComponent :
    IRetrieveEntityCollectionOperations<IHealthActionModel>, INavigable<Guid, IHealthActionCollectionComponent>,
    IComponent<IHealthActionListContext>
{
}
