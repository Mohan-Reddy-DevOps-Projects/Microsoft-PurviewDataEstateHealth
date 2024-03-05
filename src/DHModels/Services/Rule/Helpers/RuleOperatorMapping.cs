namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Collections.Generic;

public enum DHCheckPointType
{
    Number,
    String,
    Boolean,
    Score,
}

internal class RuleOperatorMapping
{
    private static readonly Dictionary<DHCheckPoint, DHCheckPointType> CheckPointTypeMapping =
        new()
        {
            { DHCheckPoint.Score, DHCheckPointType.Number },

            { DHCheckPoint.DataProductDescriptionLength, DHCheckPointType.Number },
            { DHCheckPoint.DataProductBusinessUseLength, DHCheckPointType.Number },
            { DHCheckPoint.DataProductOwnerCount, DHCheckPointType.Number },
            { DHCheckPoint.DataProductAllRelatedAssetsHaveOwner, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductAllRelatedAssetsHaveDQScore, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductRelatedDataAssetsCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductRelatedObjectivesCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductRelatedTermsCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductHasDataAccessPolicy, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductHasDataUsagePurpose, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductEndorsed, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductStatus, DHCheckPointType.String },
            { DHCheckPoint.DataProductHasDQScore, DHCheckPointType.Boolean },

            { DHCheckPoint.DataAssetClassificationCount, DHCheckPointType.Number },

            { DHCheckPoint.CDERelatedDataAssetsCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEOwnerCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEDescriptionLength, DHCheckPointType.Number },
            { DHCheckPoint.CDERelatedTermsCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEAllRelatedAssetsHaveClassification, DHCheckPointType.Boolean },

            { DHCheckPoint.BusinessDomainCriticalDataElementCount, DHCheckPointType.Number },

            { DHCheckPoint.DataQualityScore, DHCheckPointType.Score },
        };

    private static readonly Dictionary<DHCheckPointType, IList<DHOperator>> CheckPointTypeOperatorMapping = new()
    {
        { DHCheckPointType.Number, [DHOperator.Equal, DHOperator.NotEqual, DHOperator.GreaterThan, DHOperator.GreaterThanOrEqual, DHOperator.LessThan, DHOperator.LessThanOrEqual] },
        { DHCheckPointType.String, [DHOperator.Equal, DHOperator.NotEqual, DHOperator.IsNullOrEmpty, DHOperator.IsNotNullOrEmpty ] },
        { DHCheckPointType.Boolean, [DHOperator.IsTrue, DHOperator.IsFalse] },
        { DHCheckPointType.Score, [DHOperator.Normalize, DHOperator.Equal, DHOperator.NotEqual, DHOperator.GreaterThan, DHOperator.GreaterThanOrEqual, DHOperator.LessThan, DHOperator.LessThanOrEqual] },
    };

    public static IList<DHOperator> GetAllowedOperators(DHCheckPoint? checkPoint)
    {
        var checkPointType = GetCheckPointType(checkPoint);

        return CheckPointTypeOperatorMapping[checkPointType];
    }

    public static DHCheckPointType GetCheckPointType(DHCheckPoint? checkPoint)
    {
        ArgumentNullException.ThrowIfNull(checkPoint);

        if (CheckPointTypeMapping.TryGetValue(checkPoint.Value, out var checkPointType))
        {
            return checkPointType;
        }
        else
        {
            throw new InvalidOperationException($"CheckPoint {checkPoint} is not supported");
        }
    }
}
