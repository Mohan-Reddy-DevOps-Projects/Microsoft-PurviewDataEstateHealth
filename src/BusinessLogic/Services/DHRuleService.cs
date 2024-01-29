namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Threading.Tasks;

public class DHRuleService(DHSimpleRuleRepository dHSimpleRuleRepository)
{
    public async Task CreateDHSimpleRule(DHSimpleRule dHSimpleRule)
    {
        if (dHSimpleRule.Id == Guid.Empty)
        {
            dHSimpleRule.Id = Guid.NewGuid();
        }

        await dHSimpleRuleRepository.AddAsync(dHSimpleRule).ConfigureAwait(false);
    }
}
