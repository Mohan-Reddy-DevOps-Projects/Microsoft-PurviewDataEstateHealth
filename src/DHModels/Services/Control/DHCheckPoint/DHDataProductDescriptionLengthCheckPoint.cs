#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHCheckPoint;

using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;
using System.Collections.Generic;

internal static class DHDataProductDescriptonLengthCheckPoint
{
    public readonly static IEnumerable<DHOperator> AllowedOperators = new List<DHOperator>
    {
        DHOperator.Equal,
        DHOperator.GreaterThan,
        DHOperator.GreaterThanOrEqual,
        DHOperator.LessThan,
        DHOperator.LessThanOrEqual,
    };
}
