#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class CommonRepository<T>(IRequestHeaderContext requestHeaderContext) : IRepository<T> where T : ContainerEntityBase
{
    private Guid TenantId => requestHeaderContext.TenantId;
    private Guid AccountId => requestHeaderContext.AccountObjectId;
    private string ClientObjectId => requestHeaderContext.ClientObjectId;

    protected abstract DbContext TheDbContext { get; }
    protected abstract DbSet<T> TheDbSet { get; }

    public virtual async Task AddAsync(T entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.TenantId = this.TenantId;
        entity.AccountId = this.AccountId;

        entity.AuditLogs = new List<ContainerEntityAuditLog>
        {
            new() {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                User = this.ClientObjectId,
                Action = ContainerEntityAuditAction.Create,
            },
        };

        await this.TheDbSet.AddAsync(entity).ConfigureAwait(false);
        await this.TheDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task DeleteAsync(T entity)
    {
        this.ValidateEntityMetadata(entity);
        this.TheDbSet.Remove(entity);
        await this.TheDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await this.GetByIdAsync(id).ConfigureAwait(false);
        if (entity != null)
        {
            await this.DeleteAsync(entity).ConfigureAwait(false);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await this.TheDbSet.WithPartitionKey(this.TenantId.ToString()).ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await this.TheDbSet.WithPartitionKey(this.TenantId.ToString()).Where(x => x.Id == id).SingleOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        this.ValidateEntityMetadata(entity);

        entity.AuditLogs ??= new List<ContainerEntityAuditLog>();
        entity.AuditLogs.Add(
            new()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                User = this.ClientObjectId,
                Action = ContainerEntityAuditAction.Update,
            }
        );

        this.TheDbSet.Update(entity);
        await this.TheDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private void ValidateEntityMetadata(T entity)
    {
        if (entity.TenantId != this.TenantId)
        {
            throw new InvalidOperationException($"Entity's tenant id ({entity.TenantId}) is not identical with the tenant id ({this.TenantId}) from the HTTP request headers!");
        }

        if (entity.AccountId != this.AccountId)
        {
            throw new InvalidOperationException($"Entity's account id ({entity.AccountId}) is not identical with the account id ({this.AccountId}) from the HTTP request headers!");
        }
    }
}
