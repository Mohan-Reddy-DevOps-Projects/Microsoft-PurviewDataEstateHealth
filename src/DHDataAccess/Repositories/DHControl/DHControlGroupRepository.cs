#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DHControlGroupRepository(ControlDBContext controlDBContext) : IRepository<DHControlGroup>
{
    public async Task AddAsync(DHControlGroup entity)
    {
        await controlDBContext.DHControlGroups.AddAsync(entity).ConfigureAwait(false);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(DHControlGroup entity)
    {
        controlDBContext.DHControlGroups.Remove(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<DHControlGroup>> GetAllAsync()
    {
        return await controlDBContext.DHControlGroups.ToListAsync().ConfigureAwait(false);
    }

    public async Task<DHControlGroup?> GetByIdAsync(Guid id)
    {
        return await controlDBContext.DHControlGroups.FindAsync(id).ConfigureAwait(false);
    }

    public async Task UpdateAsync(DHControlGroup entity)
    {
        controlDBContext.DHControlGroups.Update(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
