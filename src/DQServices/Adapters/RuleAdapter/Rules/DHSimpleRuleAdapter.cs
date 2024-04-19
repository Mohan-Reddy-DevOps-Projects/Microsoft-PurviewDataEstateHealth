namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

internal class DHSimpleRuleAdapter
{
    public static string ToDqExpression(RuleAdapterContext ruleAdapterContext, DHSimpleRuleWrapper simpleRule)
    {
        var fieldExp = RuleFieldAdapter.ToDqExpression(ruleAdapterContext, simpleRule.CheckPoint.Value);
        var valueExp = RuleValueAdapter.ToDqExpression(simpleRule.CheckPoint.Value, simpleRule.Operand);

        return SimpleRuleOperatorAdapter.ToDqExpression(simpleRule.Operator.Value, fieldExp, valueExp);
    }
}
