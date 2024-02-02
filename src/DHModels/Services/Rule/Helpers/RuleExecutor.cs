#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Linq;

internal static class RuleExecutor
{
    public static bool Execute<TPayload>(DHSimpleRuleWrapper rule, TPayload payload)
    {
        switch (rule.CheckPoint, payload)
        {
            case (DHCheckPoints.Score, decimal scorePayload):
                {
                    var score = DHScoreCheckPoint.ExtractOperand(scorePayload);
                    var theOperator = rule.Operator;

                    if (!DHScoreCheckPoint.AllowedOperators.Contains(theOperator))
                    {
                        throw new ArgumentException($@"Operator ""{theOperator}"" is not supported in the check point ""{nameof(DHScoreCheckPoint)}""");
                    }
                    var operand = default(decimal);

                    // The operator has to stay within the limits set by DHScoreCheckPoint.AllowedOperators.
                    switch (theOperator)
                    {
                        case DHOperator.Equal:
                            operand = ToDecimal(rule.Operand);
                            return score == operand;
                        case DHOperator.GreaterThan:
                            operand = ToDecimal(rule.Operand);
                            return score > operand;
                        case DHOperator.GreaterThanOrEqual:
                            operand = ToDecimal(rule.Operand);
                            return score >= operand;
                        case DHOperator.LessThan:
                            operand = ToDecimal(rule.Operand);
                            return score < operand;
                        case DHOperator.LessThanOrEqual:
                            operand = ToDecimal(rule.Operand);
                            return score <= operand;
                        default:
                            throw new NotImplementedException();
                    }
                }
            case DHCheckPoints.DataProductDescriptionContent:
            case DHCheckPoints.DataProductDescriptionLength:
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
            DHRuleGroupOperator.AND => ruleResults.All(result => result),
            DHRuleGroupOperator.OR => ruleResults.Any(result => result),
            DHRuleGroupOperator.NOT => !ruleResults.Any(result => result),
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
            _ => throw new ArgumentException($@"Expect a number type in (decimal, int, long, double, float), but got a ""{value?.GetType().Name}"""),
        };
    }
}
