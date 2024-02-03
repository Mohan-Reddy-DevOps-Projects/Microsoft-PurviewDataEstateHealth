namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System.Collections.Generic;

internal static class DHDataProductDescriptonContentCheckPoint
{
    public readonly static IEnumerable<DHOperator> AllowedOperators =
    [
        DHOperator.Equal,
        DHOperator.Contains,
        DHOperator.IsEmpty,
        DHOperator.IsNull,
    ];
}
