namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using System;
using System.Globalization;
using System.Linq;

internal static class RuleValidator
{
    public static void Validate(DHSimpleRuleWrapper rule, DHRuleSourceType ruleSourceType)
    {
        var allowedCheckPoints = RuleOperatorMapping.GetAllowedCheckPoints(ruleSourceType);

        if (!rule.CheckPoint.HasValue || !allowedCheckPoints.Contains(rule.CheckPoint.Value))
        {
            throw new EntityValidationException(String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEnumPropertyValueNotValid,
                rule.CheckPoint.ToString(),
                DHSimpleRuleWrapper.keyCheckPoint,
                String.Join(", ", allowedCheckPoints.Select(i => i.ToString()))));
        }

        var allowedOperators = RuleOperatorMapping.GetAllowedOperators(rule.CheckPoint);

        if (!rule.Operator.HasValue || !allowedOperators.Contains(rule.Operator.Value))
        {
            throw new EntityValidationException(String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEnumPropertyValueNotValid,
                rule.Operator.ToString(),
                $"{rule.CheckPoint}/{DHSimpleRuleWrapper.keyOperator}",
                String.Join(", ", allowedCheckPoints.Select(i => i.ToString()))));
        }
    }

    public static void Validate(DHRuleExpressionWrapper rule, DHRuleSourceType ruleSourceType)
    {
        throw new NotImplementedException();
    }

    public static void Validate(DHRuleGroupWrapper ruleGroup, DHRuleSourceType ruleSourceType)
    {
        foreach (var rule in ruleGroup.Rules)
        {
            Validate(rule, ruleSourceType);
        }
    }

    public static void Validate(DHRuleBaseWrapper rule, DHRuleSourceType ruleSourceType)
    {
        switch (rule.Type)
        {
            case DHRuleBaseWrapperDerivedTypes.SimpleRule:
                Validate((DHSimpleRuleWrapper)rule, ruleSourceType);
                break;
            case DHRuleBaseWrapperDerivedTypes.ExpressionRule:
                Validate((DHRuleExpressionWrapper)rule, ruleSourceType);
                break;
            case DHRuleBaseWrapperDerivedTypes.Group:
                Validate((DHRuleGroupWrapper)rule, ruleSourceType);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
