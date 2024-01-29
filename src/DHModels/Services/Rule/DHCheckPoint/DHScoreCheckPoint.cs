#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System.Collections.Generic;

internal static class DHScoreCheckPoint
{
    public static IEnumerable<DHOperator> AllowedOperators { get; set; } = new List<DHOperator>
    {
        DHOperator.Equal,
        DHOperator.GreaterThan,
        DHOperator.GreaterThanOrEqual,
        DHOperator.LessThan,
        DHOperator.LessThanOrEqual,
    };

    public static decimal ExtractOperand(decimal payload)
    {
        return payload;
    }
}
