// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure.Data.Tables;

internal abstract class TableEntityConverter<T, TEntity>
    where TEntity : class, ITableEntity
{
    public abstract TEntity ToEntity(T model, string partitionId);

    public abstract T ToModel(TEntity entity);
}
