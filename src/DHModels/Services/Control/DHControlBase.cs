#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[CosmosDBContainer("DHControl")]
public abstract class DHControlBase : ContainerEntityBase
{
    [JsonProperty("name", Required = Required.Always)]
    public required string Name { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("owners")]
    public IList<string> Owners { get; set; } = new List<string>();

    [JsonProperty("type", Required = Required.Always)]
    [CosmosDBEnumString]
    public abstract required DHControlType Type { get; set; }

    [JsonProperty("reserved")]
    public bool Reserved { get; set; } = false;

    [JsonProperty("fallbackStatusPaletteId")]
    public Guid? FallbackStatusPaletteId { get; set; }

    [JsonProperty("statusPaletteRules")]
    public IEnumerable<DHRuleOrGroupBase> StatusPaletteRules { get; set; } = new List<DHRuleOrGroupBase>();
}

public enum DHControlType
{
    Group,
    Node,
}
