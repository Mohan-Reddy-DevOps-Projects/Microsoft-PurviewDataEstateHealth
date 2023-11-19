// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading;
using global::Azure.Data.Tables;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

internal abstract class StorageAccountRepository<T, TEntity> : IStorageAccountRepository<T>
    where TEntity : Models.EntityModel, ITableEntity
{
    private readonly ITableStorageClient<AccountStorageTableConfiguration> tableStorageClient;
    private readonly TableEntityConverter<T, TEntity> converter;
    private readonly AccountStorageTableConfiguration tableConfiguration;

    public StorageAccountRepository(ITableStorageClient<AccountStorageTableConfiguration> tableStorageClient, TableEntityConverter<T, TEntity> converter, IOptions<AccountStorageTableConfiguration> tableConfiguration)
    {
        this.tableStorageClient = tableStorageClient;
        this.converter = converter;
        this.tableConfiguration = tableConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task<T> Create(T model, string partitionKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(partitionKey, nameof(partitionKey));

        TEntity entity = this.converter.ToEntity(model, partitionKey);
        await this.tableStorageClient.AddEntityAsync(this.tableConfiguration.TableName, entity, cancellationToken);

        return this.converter.ToModel(entity);
    }

    /// <inheritdoc/>
    public async Task Delete(StorageAccountLocator entityLocator, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entityLocator, nameof(entityLocator));

        await this.tableStorageClient.DeleteEntityAsync(this.tableConfiguration.TableName, entityLocator.PartitionId, entityLocator.Name, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T> GetSingle(StorageAccountLocator entityLocator, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entityLocator, nameof(entityLocator));

        TEntity existingStorageEntity = await this.tableStorageClient.GetEntityIfExistsAsync<TEntity>(this.tableConfiguration.TableName, entityLocator.PartitionId.ToString(), entityLocator.Name, cancellationToken);

        return this.converter.ToModel(existingStorageEntity);
    }

    /// <inheritdoc/>
    public async Task Update(T model, string partitionKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(partitionKey, nameof(partitionKey));

        TEntity entity = this.converter.ToEntity(model, partitionKey);
        await this.tableStorageClient.UpdateEntityAsync(this.tableConfiguration.TableName, entity, cancellationToken);
    }
}
