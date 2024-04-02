namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;

internal static class SimpleRuleOperatorAdapter
{
    public static string ToDqExpression(DHOperator ruleOperator, string fieldVal, string operandVal)
    {
        return ruleOperator switch
        {
            DHOperator.Equal => $"{fieldVal} === {operandVal}",
            DHOperator.NotEqual => $"{fieldVal} != {operandVal}",
            DHOperator.GreaterThan => $"{fieldVal} > {operandVal}",
            DHOperator.GreaterThanOrEqual => $"{fieldVal} >= {operandVal}",
            DHOperator.LessThan => $"{fieldVal} < {operandVal}",
            DHOperator.LessThanOrEqual => $"{fieldVal} <= {operandVal}",
            DHOperator.IsNullOrEmpty => $"isNull({fieldVal}) || fieldVal === ''",
            DHOperator.IsNotNullOrEmpty => $"!(isNull({fieldVal}) || fieldVal === '')",
            DHOperator.IsTrue => $"notNull({fieldVal}) && {fieldVal} === true",
            DHOperator.IsFalse => $"isNull({fieldVal}) || {fieldVal} === false",
            _ => throw new NotImplementedException("Rule operator: " + ruleOperator.ToString())
        };
    }
}