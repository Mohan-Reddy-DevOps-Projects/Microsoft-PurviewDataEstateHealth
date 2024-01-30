#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DHControlNodeRepository(ControlDBContext controlDBContext) : IRepository<DHControlNode>
{
    public async Task AddAsync(DHControlNode entity)
    {
        await controlDBContext.DHControlNodes.AddAsync(entity).ConfigureAwait(false);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(DHControlNode entity)
    {
        controlDBContext.DHControlNodes.Remove(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<DHControlNode>> GetAllAsync()
    {
        return await controlDBContext.DHControlNodes.ToListAsync().ConfigureAwait(false);
    }

    public async Task<DHControlNode?> GetByIdAsync(Guid id)
    {
        return await controlDBContext.DHControlNodes.FindAsync(id).ConfigureAwait(false);
    }

    public async Task UpdateAsync(DHControlNode entity)
    {
        controlDBContext.DHControlNodes.Update(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
