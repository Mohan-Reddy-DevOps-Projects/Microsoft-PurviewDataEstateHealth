#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

internal class DHRuleGroup
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("groupOperator")]
    public required DHRuleGroupOperator GroupOperator { get; set; }

    [JsonProperty("rules")]
    public required IEnumerable<DHRuleBase> Rules { get; set; }

    [JsonProperty("groups")]
    public required IEnumerable<DHRuleGroup> Groups { get; set; }
}

internal enum DHRuleGroupOperator
{
    AND,
    OR,
    NOT,
}
