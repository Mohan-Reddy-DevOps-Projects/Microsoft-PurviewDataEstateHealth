// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure.Data.Tables;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Table storage client interface
/// </summary>
internal interface ITableStorageClient<TConfig> where TConfig : StorageTableConfiguration, new()
{
    /// <summary>
    /// Adds a Table Entity of type T into the Table <paramref name="tableName"/>.
    /// </summary>
    Task AddEntityAsync<T>(
        string tableName,
        T entity,
        CancellationToken cancellationToken) where T : ITableEntity;

    /// <summary>
    /// Gets the specified table entity of type T from Table <paramref name="tableName"/>. If it does not exist, return null.
    /// </summary>
    Task<T> GetEntityIfExistsAsync<T>(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken) where T : class, ITableEntity;

    /// <summary>
    /// Deletes the specified table entity from Table <paramref name="tableName"/>. This method should not fail because the entity does not exist.
    /// </summary>
    Task DeleteEntityAsync(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified table entity of type T from Table <paramref name="tableName"/>, if it exists. If the mode is Replace, the entity will be replaced. If the mode is Merge, the property values present in the entity will be merged with the existing entity
    /// </summary>
    Task UpdateEntityAsync<T>(
        string tableName,
        T entity,
        CancellationToken cancellationToken,
        TableUpdateMode mode = TableUpdateMode.Replace) where T : ITableEntity;
}
