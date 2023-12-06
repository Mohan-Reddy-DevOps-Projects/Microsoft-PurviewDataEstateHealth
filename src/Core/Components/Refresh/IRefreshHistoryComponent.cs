// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Refresh history component
/// </summary>
public interface IRefreshHistoryComponent : IRetrieveEntityCollectionOperations<Refresh>, IComponent<IRefreshHistoryContext>
{
}
