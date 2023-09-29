// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
/// The contract for components capable of retrieving entity collections.
/// </summary>
public interface IRetrieveEntityCollectionOperation<TEntity, TEnum>
    where TEntity : class
    where TEnum : struct, Enum
{
    /// <summary>
    /// Retrieves an entity collection.
    /// </summary>
    /// <param name="skipToken">Continuation Token to get next set of results</param>
    /// <param name="enumFilter"></param>
    /// <returns>A task that resolves to the entity collection.</returns>
    Task<IBatchResults<TEntity>> Get(string skipToken = null, Nullable<TEnum> enumFilter = null);
}
