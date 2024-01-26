#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHCheckPoint;
using Newtonsoft.Json;
using System;

public abstract class DHRuleBase
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("type")]
    public required DHRuleType Type { get; set; }

    [JsonProperty("checkPoint")]
    public required DHCheckPoint CheckPoint { get; set; }
}

public enum DHRuleType
{
    SimpleRule,
    ExpressionRule,
}
