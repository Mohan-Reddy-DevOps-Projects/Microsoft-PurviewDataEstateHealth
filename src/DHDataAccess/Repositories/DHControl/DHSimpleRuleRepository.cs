namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DHSimpleRuleRepository(ControlDBContext controlDBContext) : IRepository<DHSimpleRule>
{
    public async Task AddAsync(DHSimpleRule entity)
    {
        await controlDBContext.DHSimpleRules.AddAsync(entity).ConfigureAwait(false);
        await controlDBContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task DeleteAsync(DHSimpleRule entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DHSimpleRule>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DHSimpleRule> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(DHSimpleRule entity)
    {
        throw new NotImplementedException();
    }
}
