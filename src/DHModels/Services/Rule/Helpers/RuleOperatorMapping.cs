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

public enum DHRuleSourceType
{
    ControlNode,
    AssessmentMQDataProduct,
    AssessmentMQCDE,
    AssessmentMQBusinessDomain,
    AssessmentDQDataProduct,
}

public class RuleOperatorMapping
{
    private static readonly Dictionary<DHRuleSourceType, IList<DHCheckPoint>> SourceAvailableCheckPointsMapping =
        new()
        {
            { DHRuleSourceType.ControlNode, [
                DHCheckPoint.Score,
            ] },
            { DHRuleSourceType.AssessmentDQDataProduct, [
                DHCheckPoint.DataQualityScore,
                DHCheckPoint.DataQualityAccuracyScore,
                DHCheckPoint.DataQualityTimelinessScore,
                DHCheckPoint.DataQualityConformityScore,
                DHCheckPoint.DataQualityConsistencyScore,
                DHCheckPoint.DataQualityCompletenessScore,
                DHCheckPoint.DataQualityUniquenessScore,
            ] },
            { DHRuleSourceType.AssessmentMQDataProduct, [
                DHCheckPoint.DataProductDescriptionLength,
                DHCheckPoint.DataProductBusinessUseLength,
                DHCheckPoint.DataProductOwnerCount,
                DHCheckPoint.DataProductAllRelatedAssetsHaveOwner,
                DHCheckPoint.DataProductAllRelatedAssetsHaveDQScore,
                DHCheckPoint.DataProductRelatedDataAssetsCount,
                DHCheckPoint.DataProductRelatedDataAssetsWithClassificationCount,
                DHCheckPoint.DataProductRelatedObjectivesCount,
                DHCheckPoint.DataProductRelatedTermsCount,
                DHCheckPoint.DataProductHasDataAccessPolicy,
                DHCheckPoint.DataProductHasDataUsagePurpose,
                DHCheckPoint.DataProductTermsOfUseCount,
                DHCheckPoint.DataProductEndorsed,
                DHCheckPoint.DataProductStatus,
                DHCheckPoint.DataProductHasDQScore,
                DHCheckPoint.DataProductDomainDescriptionLength,
                DHCheckPoint.DataProductDomainHasOwner,
                DHCheckPoint.DataProductAllRelatedTermsMinimalDescriptionLength,
                //DHCheckPoint.DataProductAllRelatedTermsHaveOwner,
            ] },
            { DHRuleSourceType.AssessmentMQCDE, [
                DHCheckPoint.CDERelatedDataAssetsCount,
                DHCheckPoint.CDEOwnerCount,
                DHCheckPoint.CDEDescriptionLength,
                DHCheckPoint.CDERelatedTermsCount,
                DHCheckPoint.CDEAllRelatedAssetsHaveClassification,
            ]},
            {DHRuleSourceType.AssessmentMQBusinessDomain, [
                DHCheckPoint.BusinessDomainCriticalDataElementCount,
            ] },
        };

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
            { DHCheckPoint.DataProductRelatedDataAssetsWithClassificationCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductRelatedObjectivesCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductRelatedTermsCount, DHCheckPointType.Number},
            { DHCheckPoint.DataProductHasDataAccessPolicy, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductHasDataUsagePurpose, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductTermsOfUseCount, DHCheckPointType.Number },
            { DHCheckPoint.DataProductEndorsed, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductStatus, DHCheckPointType.String },
            { DHCheckPoint.DataProductHasDQScore, DHCheckPointType.Boolean },
            { DHCheckPoint.DataProductDomainDescriptionLength, DHCheckPointType.Number},
            { DHCheckPoint.DataProductDomainHasOwner, DHCheckPointType.Boolean},
            { DHCheckPoint.DataProductAllRelatedTermsMinimalDescriptionLength, DHCheckPointType.Number},
            //{ DHCheckPoint.DataProductAllRelatedTermsHaveOwner, DHCheckPointType.Boolean},

            { DHCheckPoint.CDERelatedDataAssetsCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEOwnerCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEDescriptionLength, DHCheckPointType.Number },
            { DHCheckPoint.CDERelatedTermsCount, DHCheckPointType.Number },
            { DHCheckPoint.CDEAllRelatedAssetsHaveClassification, DHCheckPointType.Boolean },

            { DHCheckPoint.BusinessDomainCriticalDataElementCount, DHCheckPointType.Number },

            { DHCheckPoint.DataQualityScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityAccuracyScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityTimelinessScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityConformityScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityConsistencyScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityCompletenessScore, DHCheckPointType.Score },
            { DHCheckPoint.DataQualityUniquenessScore, DHCheckPointType.Score },
        };

    private static readonly Dictionary<DHCheckPointType, IList<DHOperator>> CheckPointTypeOperatorMapping = new()
    {
        { DHCheckPointType.Number, [
            DHOperator.Equal,
            DHOperator.NotEqual,
            DHOperator.GreaterThan,
            DHOperator.GreaterThanOrEqual,
            DHOperator.LessThan,
            DHOperator.LessThanOrEqual
            ] },
        { DHCheckPointType.String, [
            DHOperator.Equal,
            DHOperator.NotEqual,
            DHOperator.IsNullOrEmpty,
            DHOperator.IsNotNullOrEmpty
            ] },
        { DHCheckPointType.Boolean, [
            DHOperator.IsTrue,
            DHOperator.IsFalse
            ] },
        { DHCheckPointType.Score, [
            DHOperator.Normalize,
            DHOperator.Equal,
            DHOperator.NotEqual,
            DHOperator.GreaterThan,
            DHOperator.GreaterThanOrEqual,
            DHOperator.LessThan,
            DHOperator.LessThanOrEqual
            ] },
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

    public static IList<DHCheckPoint> GetAllowedCheckPoints(DHRuleSourceType sourceType)
    {
        return SourceAvailableCheckPointsMapping[sourceType] ?? [];
    }
}
