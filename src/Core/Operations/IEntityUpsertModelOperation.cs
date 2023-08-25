// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// Used for upserting an entity based on the model passed in.
/// </summary>
public interface IEntityUpsertModelOperation<TModel>
{
    /// <summary>
    /// Upsert an entity based on the model that is passed in.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>A task containing the updated model as a result.</returns>
    Task<TModel> UpsertModel(TModel model);
}
