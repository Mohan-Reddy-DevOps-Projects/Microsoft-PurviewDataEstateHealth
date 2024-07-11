namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class CommonHttpContextRepository<TEntity>(
    IRequestHeaderContext requestHeaderContext, IDataEstateHealthRequestLogger logger, CosmosMetricsTracker cosmosMetricsTracker)
    : CommonRepository<TEntity>(logger, cosmosMetricsTracker), IHttpContextRepository<TEntity>
    where TEntity : BaseEntityWrapper, IContainerEntityWrapper
{
    private string TenantId => requestHeaderContext.TenantId.ToString();
    private string AccountId => requestHeaderContext.AccountObjectId.ToString();
    protected PartitionKey TenantPartitionKey => new(this.TenantId);
    protected AccountIdentifier AccountIdentifier => new() { TenantId = this.TenantId, AccountId = this.AccountId };

    /// <inheritdoc />
    public Task<TEntity> AddAsync(TEntity entity)
    {
        return this.AddAsync(entity, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems, IReadOnlyCollection<TEntity> IgnoredItems)> AddAsync(IReadOnlyList<TEntity> entities)
    {
        return this.AddAsync(entities, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> UpdateAsync(IReadOnlyList<TEntity> entities)
    {
        return this.UpdateAsync(entities, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task DeleteAsync(TEntity entity)
    {
        return this.DeleteAsync(entity.Id);
    }

    /// <inheritdoc />
    public Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> DeleteAsync(IReadOnlyList<TEntity> entities)
    {
        return this.DeleteAsync(entities, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string id)
    {
        return this.DeleteAsync(id, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task DeprovisionAsync()
    {
        return this.DeprovisionAsync(this.AccountIdentifier);
    }

    public Task DeprovisionDEHAsync()
    {
        return this.DeprovisionDEHAsync(this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return this.GetAllAsync(this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(string id)
    {
        return this.GetByIdAsync(id, this.AccountIdentifier);
    }

    /// <inheritdoc />
    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        return this.UpdateAsync(entity, this.AccountIdentifier);
    }
}
