#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;
using System;
using System.Linq;

internal static class RuleExecutor
{
    public static bool Execute<TPayload>(DHRuleBase rule, TPayload payload)
    {
        switch (rule.CheckPoint, payload)
        {
            case (DHCheckPoint.Score, decimal scorePayload):
                {
                    var score = DHScoreCheckPoint.ExtractOperand(scorePayload);
                    switch (rule)
                    {
                        case DHSimpleRule simpleRule:
                            {
                                var theOperator = simpleRule.Operator;

                                if (!DHScoreCheckPoint.AllowedOperators.Contains(theOperator))
                                {
                                    throw new ArgumentException($@"Operator ""{theOperator}"" is not supported in the check point ""{nameof(DHScoreCheckPoint)}""");
                                }
                                var operand = default(decimal);

                                // The operator has to stay within the limits set by DHScoreCheckPoint.AllowedOperators.
                                switch (theOperator)
                                {
                                    case DHOperator.Equal:
                                        operand = ToDecimal(simpleRule.Operand);
                                        return score == operand;
                                    case DHOperator.GreaterThan:
                                        operand = ToDecimal(simpleRule.Operand);
                                        return score > operand;
                                    case DHOperator.GreaterThanOrEqual:
                                        operand = ToDecimal(simpleRule.Operand);
                                        return score >= operand;
                                    case DHOperator.LessThan:
                                        operand = ToDecimal(simpleRule.Operand);
                                        return score < operand;
                                    case DHOperator.LessThanOrEqual:
                                        operand = ToDecimal(simpleRule.Operand);
                                        return score <= operand;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        case DHExpressionRule expressionRule:
                        default:
                            {
                                throw new NotImplementedException();
                            }
                    }
                }
            case DHCheckPoint.DataProductDescriptionContent:
            case DHCheckPoint.DataProductDescriptionLength:
            default:
                throw new NotImplementedException();
        }
    }

    public static bool Execute<TPayload>(DHRuleGroup ruleGroup, TPayload payload)
    {
        var ruleResults = ruleGroup.Rules.Select(rule => Execute(rule, payload));
        var ruleGroupResults = ruleGroup.Groups.Select(group => Execute(group, payload));
        var finalResult = ruleGroup.GroupOperator switch
        {
            DHRuleGroupOperator.AND => ruleResults.All(result => result) && ruleGroupResults.All(result => result),
            DHRuleGroupOperator.OR => ruleResults.Any(result => result) || ruleGroupResults.Any(result => result),
            DHRuleGroupOperator.NOT => !ruleResults.Any(result => result) && !ruleGroupResults.Any(result => result),
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
