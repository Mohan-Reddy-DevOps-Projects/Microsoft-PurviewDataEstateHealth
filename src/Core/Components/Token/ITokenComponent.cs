// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Token Component contract.
/// </summary>
public interface ITokenComponent : IComponent<ITokenContext>
{
    /// <summary>
    /// Get token.
    /// </summary>
    /// <param name="tokenModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<EmbedToken> Get(TokenModel tokenModel, CancellationToken cancellationToken);
}
