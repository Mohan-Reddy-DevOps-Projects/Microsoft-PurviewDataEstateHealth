#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHCheckPoint;

using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;
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
