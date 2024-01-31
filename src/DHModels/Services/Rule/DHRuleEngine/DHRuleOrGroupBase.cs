#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class DHRuleOrGroupBase
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("type")]
    [CosmosDBEnumString]
    public virtual DHRuleOrGroupType Type { get; set; }

    [JsonProperty("additionalProperties")]
    public JObject? AdditionalProperties { get; set; }

    [JsonProperty("reserved")]
    public bool Reserved { get; set; } = false;
}

public enum DHRuleOrGroupType
{
    Rule,
    Group
}
