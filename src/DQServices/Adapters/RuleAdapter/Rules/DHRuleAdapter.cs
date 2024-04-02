// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

public static class DHRuleAdapter
{
    public static string ToDQExpression(RuleAdapterContext ruleAdapterContext, DHRuleBaseWrapper controlRule)
    {
        if (controlRule is DHRuleGroupWrapper controlRuleGroup)
        {
            return DHRuleGroupAdapter.ToDqExpression(ruleAdapterContext, controlRuleGroup);
        }
        else if (controlRule is DHSimpleRuleWrapper controlRuleSimple)
        {
            return DHSimpleRuleAdapter.ToDqExpression(ruleAdapterContext, controlRuleSimple);
        }
        else
        {
            throw new ServiceException($"Invalid control rule type to parse, {controlRule.Type}");
        }
    }
}
