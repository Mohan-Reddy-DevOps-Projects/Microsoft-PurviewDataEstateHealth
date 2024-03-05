namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHCheckPoint
{
    // ControlNode
    Score,

    // Assessment - MQ - DataProduct
    DataProductDescriptionLength,
    DataProductBusinessUseLength,
    DataProductOwnerCount,
    DataProductAllRelatedAssetsHaveOwner,
    DataProductAllRelatedAssetsHaveDQScore,
    DataProductRelatedDataAssetsCount,
    DataProductRelatedObjectivesCount,
    DataProductRelatedTermsCount,
    DataProductHasDataAccessPolicy,
    DataProductHasDataUsagePurpose,
    DataProductEndorsed,
    DataProductStatus,
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
