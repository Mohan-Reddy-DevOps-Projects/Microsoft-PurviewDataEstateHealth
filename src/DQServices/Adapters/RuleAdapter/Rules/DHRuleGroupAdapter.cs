namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System.Linq;

internal class DHRuleGroupAdapter
{
    public static string ToDqExpression(RuleAdapterContext ruleAdapterContext, DHRuleGroupWrapper groupRule)
    {
        return groupRule.GroupOperator! switch
        {
            DHRuleGroupOperator.And => $"({DHRuleAdapter.ToDQExpression(ruleAdapterContext, groupRule.Rules.ElementAt(0))}) && ({DHRuleAdapter.ToDQExpression(ruleAdapterContext, groupRule.Rules.ElementAt(1))})",
            DHRuleGroupOperator.Or => $"({DHRuleAdapter.ToDQExpression(ruleAdapterContext, groupRule.Rules.ElementAt(0))}) || ({DHRuleAdapter.ToDQExpression(ruleAdapterContext, groupRule.Rules.ElementAt(1))})",
            DHRuleGroupOperator.Not => $"!({DHRuleAdapter.ToDQExpression(ruleAdapterContext, groupRule.Rules.ElementAt(0))})",
            _ => throw new System.NotImplementedException("Group operator: " + groupRule.GroupOperator)
        };
    }
}
