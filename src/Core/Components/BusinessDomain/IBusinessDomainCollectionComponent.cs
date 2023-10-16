// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing business domain collections.
/// </summary>
public interface IBusinessDomainCollectionComponent : IRetrieveEntityCollectionOperations<IBusinessDomainModel>, IComponent<IBusinessDomainListContext>
{
}

