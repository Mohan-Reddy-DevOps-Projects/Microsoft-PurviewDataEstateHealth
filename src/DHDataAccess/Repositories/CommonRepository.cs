namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class CommonRepository<TEntity>(IRequestHeaderContext requestHeaderContext) : IRepository<TEntity>
    where TEntity : BaseEntityWrapper, IContainerEntityWrapper
{
    protected string TenantId => requestHeaderContext.TenantId.ToString();
    protected string AccountId => requestHeaderContext.AccountObjectId.ToString();
    protected PartitionKey TenantPartitionKey => new(this.TenantId);

    protected abstract Container CosmosContainer { get; }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        this.PopulateMetadataForEntity(entity);
        var response = await this.CosmosContainer.CreateItemAsync(entity, this.TenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities)
    {
        if (!entities.Any())
        {
            return [];
        }

        // Materialize the incoming sequence into a List to ensure it's only evaluated once
        var materializedEntities = entities.ToList();

        foreach (var entity in materializedEntities)
        {
            this.PopulateMetadataForEntity(entity);
        }
        var batch = this.CosmosContainer.CreateTransactionalBatch(this.TenantPartitionKey);

        foreach (var entity in materializedEntities)
        {
            batch.CreateItem(entity);
        }

        await batch.ExecuteAsync().ConfigureAwait(false);

        return materializedEntities;
    }

    public virtual async Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
    {
        entities.ForEach(this.PopulateMetadataForEntity);
        var batch = this.CosmosContainer.CreateTransactionalBatch(this.TenantPartitionKey);

        foreach (var entity in entities)
        {
            batch.UpsertItem(entity);
        }

        await batch.ExecuteAsync().ConfigureAwait(false);

        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity)
    {
        return await this.DeleteAsync(entity.Id).ConfigureAwait(false);
    }

    public async Task<TEntity> DeleteAsync(string id)
    {
        var response = await this.CosmosContainer.DeleteItemAsync<TEntity>(id, this.TenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var feedIterator = this.CosmosContainer.GetItemLinqQueryable<TEntity>(
            requestOptions: new QueryRequestOptions { PartitionKey = this.TenantPartitionKey }
        ).Where(x => true).ToFeedIterator();

        var results = new List<TEntity>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        return results;
    }

    public virtual async Task<TEntity?> GetByIdAsync(string id)
    {
        var response = await this.CosmosContainer.ReadItemAsync<TEntity>(id, this.TenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        this.PopulateMetadataForEntity(entity);
        var response = await this.CosmosContainer.UpsertItemAsync(entity, this.TenantPartitionKey).ConfigureAwait(false);
        return response.Resource;
    }

    private void PopulateMetadataForEntity(TEntity entity)
    {
        entity.TenantId = this.TenantId;
        entity.AccountId = this.AccountId;
    }
}
