#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class MQAssessment
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("targetEntityType")]
    public required MQAssessmentTargetEntityType TargetEntityType { get; set; }

    [JsonProperty("rules")]
    public required IEnumerable<DHRuleOrGroupBase> Rules { get; set; }

    [JsonProperty("aggregation")]
    public required MQAssessmentAggregationBase Aggregation { get; set; }

    [JsonProperty("reserved")]
    public bool Reserved { get; set; } = false;
}

public enum MQAssessmentTargetEntityType
{
    DataProduct
}
