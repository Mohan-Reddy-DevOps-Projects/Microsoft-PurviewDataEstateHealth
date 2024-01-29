#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Newtonsoft.Json;

public abstract class DHRuleBase : DHRuleOrGroupBase
{
    [JsonProperty("ruleType")]
    [CosmosDBEnumString]
    public abstract DHRuleType RuleType { get; set; }

    [JsonProperty("checkPoint")]
    [CosmosDBEnumString]
    public required DHCheckPoint CheckPoint { get; set; }

    [JsonProperty("type")]
    public override DHRuleOrGroupType Type
    {
        get => DHRuleOrGroupType.Rule;
        set { }
    }
}

public enum DHRuleType
{
    SimpleRule,
    ExpressionRule,
}
