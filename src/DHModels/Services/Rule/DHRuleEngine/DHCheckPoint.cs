namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHCheckPoint
{
    Unknown,

    // ControlNode
    Score,

    // Assessment - DataProduct
    DataProductClassificationCount,
    DataProductOwnerCount,
    DataProductRelatedDataAssetsCount,
    DataProductHasDQScore,
    DataProductRelatedObjectivesCount,

    // Assessment - BusinessDomain
    BusinessDomainHasCriticalData,

}
