namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class CommonRepository<TEntity>() : IRepository<TEntity>
    where TEntity : BaseEntityWrapper, IContainerEntityWrapper
{
    protected abstract Container CosmosContainer { get; }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, string tenantId, string? accountId)
    {
        PopulateMetadataForEntity(entity, tenantId, accountId);
        var tenantPartitionKey = new PartitionKey(tenantId);
        var response = await this.CosmosContainer.CreateItemAsync(entity, tenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> AddAsync(IReadOnlyList<TEntity> entities, string tenantId, string? accountId)
    {
        if (!entities.Any())
        {
            return [];
        }

        foreach (var entity in entities)
        {
            PopulateMetadataForEntity(entity, tenantId, accountId);
        }

        var tenantPartitionKey = new PartitionKey(tenantId);

        var batch = this.CosmosContainer.CreateTransactionalBatch(tenantPartitionKey);

        foreach (var entity in entities)
        {
            batch.CreateItem(entity);
        }

        await batch.ExecuteAsync().ConfigureAwait(false);

        return entities;
    }

    /// <inheritdoc />
    public Task<TEntity> DeleteAsync(TEntity entity, string tenantId)
    {
        return this.DeleteAsync(entity.Id, tenantId);
    }

    /// <inheritdoc />
    public async Task<TEntity> DeleteAsync(string id, string tenantId)
    {
        var tenantPartitionKey = new PartitionKey(tenantId);

        var response = await this.CosmosContainer.DeleteItemAsync<TEntity>(id, tenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync(string tenantId)
    {
        var tenantPartitionKey = new PartitionKey(tenantId);

        var feedIterator = this.CosmosContainer.GetItemLinqQueryable<TEntity>(
            requestOptions: new QueryRequestOptions { PartitionKey = tenantPartitionKey }
        ).Where(x => true).ToFeedIterator();

        var results = new List<TEntity>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, string tenantId)
    {
        var tenantPartitionKey = new PartitionKey(tenantId);

        var response = await this.CosmosContainer.ReadItemAsync<TEntity>(id, tenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, string tenantId, string? accountId)
    {
        PopulateMetadataForEntity(entity, tenantId, accountId);
        var tenantPartitionKey = new PartitionKey(tenantId);

        var response = await this.CosmosContainer.UpsertItemAsync(entity, tenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> UpdateAsync(IReadOnlyList<TEntity> entities, string tenantId, string? accountId)
    {
        if (!entities.Any())
        {
            return [];
        }

        foreach (var entity in entities)
        {
            PopulateMetadataForEntity(entity, tenantId, accountId);
        }

        var tenantPartitionKey = new PartitionKey(tenantId);

        var batch = this.CosmosContainer.CreateTransactionalBatch(tenantPartitionKey);

        foreach (var entity in entities)
        {
            batch.UpsertItem(entity);
        }

        await batch.ExecuteAsync().ConfigureAwait(false);

        return entities;
    }

    private static void PopulateMetadataForEntity(TEntity entity, string tenantId, string? accountId)
    {
        entity.TenantId = tenantId;
        if (accountId != null)
        {
            entity.AccountId = accountId;
        }
    }
}
