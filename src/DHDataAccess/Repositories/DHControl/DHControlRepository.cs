#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DHControlRepository(ControlDBContext controlDBContext) : IRepository<DHControlBase>
{
    public async Task AddAsync(DHControlBase entity)
    {
        await controlDBContext.DHControls.AddAsync(entity).ConfigureAwait(false);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(DHControlBase entity)
    {
        controlDBContext.DHControls.Remove(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<DHControlBase>> GetAllAsync()
    {
        return await controlDBContext.DHControls.ToListAsync().ConfigureAwait(false);
    }

    public async Task<DHControlBase?> GetByIdAsync(Guid id)
    {
        return await controlDBContext.DHControls.FindAsync(id).ConfigureAwait(false);
    }

    public async Task UpdateAsync(DHControlBase entity)
    {
        controlDBContext.DHControls.Update(entity);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
