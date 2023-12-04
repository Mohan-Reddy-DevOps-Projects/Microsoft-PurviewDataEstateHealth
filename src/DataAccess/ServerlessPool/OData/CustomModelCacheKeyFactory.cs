// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Creates keys that uniquely identifies the model for a given context and the customer, and used to store and lookup
/// a cached model. Unlike the the default implementation of ModelCacheKeyFactory, which uses the context type and the designTime
/// boolean as key, we also include the schema name in the key. This is because we have multiple schemas(mapping uniquely to a customer)
/// in the same database, and thus want to cache the model for each schema separately.
/// </summary>
internal sealed class CustomModelCacheKeyFactory : IModelCacheKeyFactory
{
    /// <summary>
    /// Gets the model cache key for a given context.
    /// </summary>
    public object Create(DbContext context, bool designTime) => new CustomModelCacheKey(context, designTime);
}
