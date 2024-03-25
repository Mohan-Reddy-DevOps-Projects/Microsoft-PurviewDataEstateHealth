namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHCheckPoint
{
    // ControlNode
    Score,

    // Assessment - MQ - DataProduct
    DataProductDescriptionLength, // Supported in MDQ
    DataProductBusinessUseLength, // Supported in MDQ
    DataProductOwnerCount, // Supported in MDQ
    DataProductAllRelatedAssetsHaveOwner, // Supported in MDQ
    DataProductAllRelatedAssetsHaveDQScore, // Supported in MDQ
    DataProductRelatedDataAssetsCount, // Supported in MDQ
    DataProductRelatedDataAssetsWithClassificationCount,
    DataProductRelatedObjectivesCount, // Cannot do, catalog is not posting OKR contract to EH
    DataProductRelatedTermsCount, // Supported in MDQ
    DataProductHasDataAccessPolicy, // Supported in MDQ
    DataProductHasDataUsagePurpose, // Supported in MDQ
    DataProductTermsOfUseCount, // Supported in MDQ
    DataProductEndorsed, // Supported in MDQ
    DataProductStatus, // Supported in MDQ
    DataProductHasDQScore, // Supported in MDQ
    DataProductDomainDescriptionLength, // Supported in MDQ
    DataProductDomainHasOwner, // Supported in MDQ
    DataProductAllRelatedTermsMinimalDescriptionLength, // Supported in MDQ
    //DataProductAllRelatedTermsHaveOwner,

    // Assessment - MQ - CDE
    CDERelatedDataAssetsCount,
    CDEOwnerCount,
    CDEDescriptionLength,
    CDERelatedTermsCount,
    CDEAllRelatedAssetsHaveClassification,

    // Assessment - MQ - BusinessDomain
    BusinessDomainCriticalDataElementCount,

    // Assessment - DQ - DataProduct
    DataQualityScore,
    DataQualityAccuracyScore,
    DataQualityTimelinessScore,
    DataQualityConformityScore,
    DataQualityConsistencyScore,
    DataQualityCompletenessScore,
    DataQualityUniquenessScore,
}
