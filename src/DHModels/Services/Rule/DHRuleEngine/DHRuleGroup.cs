#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DHRuleGroup : DHRuleOrGroupBase
{
    [JsonProperty("groupOperator")]
    [CosmosDBEnumString]
    public required DHRuleGroupOperator GroupOperator { get; set; }

    [JsonProperty("rules")]
    public required IEnumerable<DHRuleOrGroupBase> Rules { get; set; }

    [JsonProperty("type")]
    public override DHRuleOrGroupType Type
    {
        get => DHRuleOrGroupType.Group;
        set { }
    }
}

public enum DHRuleGroupOperator
{
    AND,
    OR,
    NOT,
}
