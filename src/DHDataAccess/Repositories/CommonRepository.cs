namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class CommonRepository<T>(IRequestHeaderContext requestHeaderContext) : IRepository<T> where T : class, IContainerEntityWrapper
{
    public string TenantId => requestHeaderContext.TenantId.ToString();
    public string AccountId => requestHeaderContext.AccountObjectId.ToString();
    private string ClientObjectId => requestHeaderContext.ClientObjectId;

    protected abstract DbContext DBContext { get; }

    public virtual async Task<T> AddAsync(T entity)
    {
        this.PopulateMetadataForAdding(entity);

        await this.DBContext.Set<T>().AddAsync(entity).ConfigureAwait(false);
        var affectedRows = await this.DBContext.SaveChangesAsync().ConfigureAwait(false);

        return affectedRows switch
        {
            1 => entity,
            _ => throw new DBInvalidAffectedRowsException($"Unexpected number of entities ({affectedRows}) affected by the add operation!"),
        };
    }

    public virtual async Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            this.PopulateMetadataForAdding(entity);
        }

        await this.DBContext.Set<T>().AddRangeAsync(entities).ConfigureAwait(false);
        var affectedRows = await this.DBContext.SaveChangesAsync().ConfigureAwait(false);

        if (entities.Count() == affectedRows)
        {
            return entities;
        }

        throw new DBInvalidAffectedRowsException($"Unexpected number of entities ({affectedRows}) affected by the add operation!");
    }

    private async Task<T> DeleteAsyncInternal(T entity)
    {
        this.DBContext.Set<T>().Remove(entity);
        var affectedRows = await this.DBContext.SaveChangesAsync().ConfigureAwait(false);

        return affectedRows switch
        {
            1 => entity,
            _ => throw new DBInvalidAffectedRowsException($"Unexpected number of entities ({affectedRows}) affected by the delete operation!"),
        };
    }

    public virtual async Task<T> DeleteAsync(T entity)
    {
        var entityInDb = await this.EnsureOwnerShip(entity).ConfigureAwait(false);

        return await this.DeleteAsyncInternal(entityInDb).ConfigureAwait(false);
    }

    public virtual async Task<T> DeleteAsync(string id)
    {
        var entityInDb = await this.EnsureOwnerShip(id).ConfigureAwait(false);

        return await this.DeleteAsyncInternal(entityInDb).ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await this.DBContext.Set<T>().WithPartitionKey(this.TenantId).ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<T> GetByIdAsync(string id)
    {
        return (await this.DBContext.Set<T>().WithPartitionKey(this.TenantId).Where(x => x.Id == id).SingleOrDefaultAsync().ConfigureAwait(false))
            ?? throw new DBEntityNotFoundException($"Entity with id {id} does not exist in the database!");
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        var entityInDb = await this.EnsureOwnerShip(entity).ConfigureAwait(false);

        var log = new ContainerEntityAuditLogWrapper()
        {
            Time = DateTime.UtcNow,
            User = this.ClientObjectId,
            Action = ContainerEntityAuditAction.Update,
        };

        var newAuditLogs = new List<ContainerEntityAuditLogWrapper>();

        if (entityInDb?.AuditLogs != null)
        {
            newAuditLogs.AddRange(entityInDb.AuditLogs);
        }

        newAuditLogs.Add(log);

        entity.AuditLogs = newAuditLogs;

        this.DBContext.Set<T>().Update(entity);
        var affectedRows = await this.DBContext.SaveChangesAsync().ConfigureAwait(false);

        return affectedRows switch
        {
            1 => entity,
            _ => throw new DBInvalidAffectedRowsException($"Unexpected number of entities ({affectedRows}) affected by the update operation!"),
        };
    }

    private T PopulateMetadataForAdding(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }

        entity.TenantId = this.TenantId;
        entity.AccountId = this.AccountId;

        entity.AuditLogs =
        [
            new()
            {
                Time = DateTime.UtcNow,
                User = this.ClientObjectId,
                Action = ContainerEntityAuditAction.Create,
            },
        ];
        return entity;
    }

    private Task<T> EnsureOwnerShip(T entity)
    {
        return this.EnsureOwnerShip(entity.Id);
    }

    private async Task<T> EnsureOwnerShip(string id)
    {
        var entityInDb = await this.GetByIdAsync(id).ConfigureAwait(false) ?? throw new DBEntityNotFoundException($"Entity with id {id} does not exist in the database!");

        if (entityInDb.TenantId != this.TenantId)
        {
            throw new InvalidOperationException($"Entity's tenant id ({entityInDb.TenantId}) is not identical with the tenant id ({this.TenantId}) from the HTTP request headers!");
        }

        if (entityInDb.AccountId != this.AccountId)
        {
            throw new InvalidOperationException($"Entity's account id ({entityInDb.AccountId}) is not identical with the account id ({this.AccountId}) from the HTTP request headers!");
        }

        return entityInDb;
    }
}
