namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class CommonHttpContextRepository<TEntity>(IRequestHeaderContext requestHeaderContext) : CommonRepository<TEntity>, IHttpContextRepository<TEntity>
    where TEntity : BaseEntityWrapper, IContainerEntityWrapper
{
    protected string TenantId => requestHeaderContext.TenantId.ToString();
    protected string AccountId => requestHeaderContext.AccountObjectId.ToString();
    protected PartitionKey TenantPartitionKey => new(this.TenantId);

    /// <inheritdoc />
    public Task<TEntity> AddAsync(TEntity entity)
    {
        return this.AddAsync(entity, this.TenantId, this.AccountId);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> AddAsync(IReadOnlyList<TEntity> entities)
    {
        return this.AddAsync(entities, this.TenantId, this.AccountId);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> UpdateAsync(IReadOnlyList<TEntity> entities)
    {
        return this.UpdateAsync(entities, this.TenantId, this.AccountId);
    }

    /// <inheritdoc />
    public Task<TEntity> DeleteAsync(TEntity entity)
    {
        return this.DeleteAsync(entity.Id);
    }

    /// <inheritdoc />
    public Task<TEntity> DeleteAsync(string id)
    {
        return this.DeleteAsync(id, this.TenantId);
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return this.GetAllAsync(this.TenantId);
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(string id)
    {
        return this.GetByIdAsync(id, this.TenantId);
    }

    /// <inheritdoc />
    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        return this.UpdateAsync(entity, this.TenantId, this.AccountId);
    }
}
