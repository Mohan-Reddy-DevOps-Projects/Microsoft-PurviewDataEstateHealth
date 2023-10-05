// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Token Component contract.
/// </summary>
public interface ITokenComponent : IRetrieveEntityOperation<EmbedToken>,
    IComponent<ITokenContext>
{
}
