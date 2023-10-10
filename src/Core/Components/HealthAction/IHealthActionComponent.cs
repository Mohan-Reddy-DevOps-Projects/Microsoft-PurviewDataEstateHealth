// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Health action component contract.
/// </summary>
public interface IHealthActionComponent : IRetrieveEntityOperation<IHealthActionModel>, IComponent<IHealthActionContext>
{
}
