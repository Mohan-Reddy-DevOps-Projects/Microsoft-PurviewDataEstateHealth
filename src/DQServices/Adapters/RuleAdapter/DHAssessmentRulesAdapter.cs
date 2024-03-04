// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class DHAssessmentRulesAdapter
{
    public static RuleAdapterResult ToDqRules(IEnumerable<DHAssessmentRuleWrapper> controlRules)
    {
        var customRules = new List<CustomTruthRuleWrapper>();
        var ruleAdapterContext = new RuleAdapterContext();

        foreach (var controlRule in controlRules)
        {
            var customRule = new CustomTruthRuleWrapper(new JObject()
            {
                { DynamicEntityWrapper.keyType, CustomTruthRuleWrapper.EntityType },
                { DynamicEntityWrapper.keyTypeProperties, new JObject() }
            });
            customRule.Id = controlRule.Id;
            customRule.Name = controlRule.Id;
            customRule.Status = RuleStatus.Active;
            customRule.Condition = DHRuleAdapter.ToDQExpression(ruleAdapterContext, controlRule.Rule);

            customRules.Add(customRule);
        }

        return new RuleAdapterResult()
        {
            CustomRules = customRules
        };
    }
}
