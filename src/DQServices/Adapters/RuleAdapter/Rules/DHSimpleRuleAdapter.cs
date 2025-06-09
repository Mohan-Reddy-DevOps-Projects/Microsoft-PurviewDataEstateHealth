namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

internal class DHSimpleRuleAdapter
{
    public static string ToDqExpression(RuleAdapterContext ruleAdapterContext, DHSimpleRuleWrapper simpleRule)
    {
        var fieldExp = RuleFieldAdapter.ToDqExpression(ruleAdapterContext, simpleRule.CheckPoint.Value);
        var valueExp = RuleValueAdapter.ToDqExpression(simpleRule.CheckPoint.Value, simpleRule.Operand);

        var basicExp = SimpleRuleOperatorAdapter.ToDqExpression(simpleRule.Operator.Value, fieldExp, valueExp);
        switch (simpleRule.CheckPoint)
        {
            case DHCheckPoint.DataProductRelatedAssetsHaveDQScore:
                return $"notNull(DADQSDataAssetId) && ({basicExp})";
            case DHCheckPoint.DataProductRelatedAssetsOwnerCount:
                return $"notNull(ADODataAssetId) && ({basicExp})";
            case DHCheckPoint.DataProductRelatedDataAssetsWithClassificationCount:
                return $"notNull(DACDataAssetId) && ({basicExp})";
            case DHCheckPoint.DataProductAllRelatedTermsMinimalDescriptionLength:
            case DHCheckPoint.DataProductRelatedTermsDescriptionLength:
                return $"notNull(DPTGlossaryTermId) && ({basicExp})";
            default:
                return basicExp;
        }
    }
}
