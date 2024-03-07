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
    DataProductOwnerCount,
    DataProductAllRelatedAssetsHaveOwner,
    DataProductAllRelatedAssetsHaveDQScore,
    DataProductRelatedDataAssetsCount, // Supported in MDQ
    DataProductRelatedObjectivesCount,
    DataProductRelatedTermsCount,
    DataProductHasDataAccessPolicy,
    DataProductHasDataUsagePurpose,
    DataProductEndorsed, // Supported in MDQ
    DataProductStatus, // Supported in MDQ
    DataProductHasDQScore,

    // Assessment - MQ - DataAsset
    DataAssetClassificationCount,

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
}
