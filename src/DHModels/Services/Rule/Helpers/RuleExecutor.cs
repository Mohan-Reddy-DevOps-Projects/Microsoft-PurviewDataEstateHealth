namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Linq;

internal static class RuleExecutor
{
    public static bool Execute<TPayload>(DHSimpleRuleWrapper rule, TPayload payload)
    {
        var checkValue = GetCheckPointValue(rule.CheckPoint, payload);
        var theOperator = rule.Operator ?? throw new ArgumentException($@"Operator is not set in the check point ""{DHCheckPoint.Score}""");
        var allowedOperators = RuleOperatorMapping.GetAllowedOperators(rule.CheckPoint);
        if (!allowedOperators.Contains(theOperator))
        {
            throw new ArgumentException($@"Operator ""{theOperator}"" is not supported in the check point ""{DHCheckPoint.Score}""");
        }

        var checkPointType = RuleOperatorMapping.GetCheckPointType(rule.CheckPoint);

        switch (checkPointType)
        {
            case DHCheckPointType.Boolean:
                if (checkValue == null || checkValue is not bool)
                {
                    throw new ArgumentException($@"The check point ""{rule.CheckPoint}"" is not a boolean value");
                }
                var boolCheckValue = (bool)checkValue;
                {
                    return theOperator switch
                    {
                        DHOperator.Equal => boolCheckValue == Convert.ToBoolean(rule.Operand),
                        _ => throw new NotImplementedException(),
                    };
                }
            case DHCheckPointType.Number:
                var decimalCheckValue = ToDecimal(checkValue);
                var operand = ToDecimal(rule.Operand);
                return theOperator switch
                {
                    DHOperator.Equal => decimalCheckValue == operand,
                    DHOperator.GreaterThan => decimalCheckValue > operand,
                    DHOperator.GreaterThanOrEqual => decimalCheckValue >= operand,
                    DHOperator.LessThan => decimalCheckValue < operand,
                    DHOperator.LessThanOrEqual => decimalCheckValue <= operand,
                    _ => throw new NotImplementedException(),
                };
            case DHCheckPointType.String:
            default:
                throw new NotImplementedException();
        }
    }

    public static bool Execute<TPayload>(DHRuleGroupWrapper ruleGroup, TPayload payload)
    {
        var ruleResults = ruleGroup.Rules.Select(rule => rule switch
        {
            DHSimpleRuleWrapper _rule => Execute(_rule, payload),
            DHRuleGroupWrapper _group => Execute(_group, payload),
            _ => throw new NotImplementedException()
        });
        var finalResult = ruleGroup.GroupOperator switch
        {
            DHRuleGroupOperator.And => ruleResults.All(result => result),
            DHRuleGroupOperator.Or => ruleResults.Any(result => result),
            DHRuleGroupOperator.Not => !ruleResults.Any(result => result),
            _ => throw new ArgumentException($@"Unsupported group operator ""{ruleGroup.GroupOperator}"""),
        };
        return finalResult;
    }

    private static decimal ToDecimal(object? value)
    {
        return value switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            double d => (decimal)d,
            float f => (decimal)f,
            string s => decimal.Parse(s),
            _ => throw new ArgumentException($@"Expect a number type in (decimal, int, long, double, float), but got a ""{value?.GetType().Name}"""),
        };
    }

    private static object? GetCheckPointValue<TPayload>(DHCheckPoint? checkPoint, TPayload payload)
    {
        return checkPoint switch
        {
            DHCheckPoint.Score => payload,
            _ => throw new NotImplementedException(),
        };
    }
}
