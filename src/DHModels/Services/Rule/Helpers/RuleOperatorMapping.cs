namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Collections.Generic;

public enum DHCheckPointType
{
    Number,
    String,
    Boolean,
}

internal class RuleOperatorMapping
{
    private static readonly IDictionary<DHCheckPoint, DHCheckPointType> CheckPointTypeMapping =
        new Dictionary<DHCheckPoint, DHCheckPointType>
        {
            { DHCheckPoint.Score, DHCheckPointType.Number },
            { DHCheckPoint.DataProductClassificationCount, DHCheckPointType.Number },
            { DHCheckPoint.DataProductOwnerCount, DHCheckPointType.Number },
            { DHCheckPoint.DataProductRelatedDataAssetsCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductHasDQScore, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductRelatedObjectivesCount, DHCheckPointType.Number },
            { DHCheckPoint.BusinessDomainHasCriticalData, DHCheckPointType.Boolean }
        };

    private static readonly IDictionary<DHCheckPointType, IList<DHOperator>> CheckPointTypeOperatorMapping = new Dictionary<DHCheckPointType, IList<DHOperator>>
    {
        { DHCheckPointType.Number, [DHOperator.Equal, DHOperator.GreaterThan, DHOperator.GreaterThanOrEqual, DHOperator.LessThan, DHOperator.LessThanOrEqual] },
        { DHCheckPointType.String, [DHOperator.Equal, DHOperator.Contains, DHOperator.IsEmpty, DHOperator.IsNull ] },
        { DHCheckPointType.Boolean, [DHOperator.Equal] },
    };

    public static IList<DHOperator> GetAllowedOperators(DHCheckPoint checkPoint)
    {
        if (CheckPointTypeMapping.TryGetValue(checkPoint, out var checkPointType))
        {
            return CheckPointTypeOperatorMapping[checkPointType];
        }
        else
        {
            throw new InvalidOperationException($"CheckPoint {checkPoint} is not supported");
        }
    }

    public static DHCheckPointType GetCheckPointType(DHCheckPoint checkPoint)
    {
        if (CheckPointTypeMapping.TryGetValue(checkPoint, out var checkPointType))
        {
            return checkPointType;
        }
        else
        {
            throw new InvalidOperationException($"CheckPoint {checkPoint} is not supported");
        }
    }
}
