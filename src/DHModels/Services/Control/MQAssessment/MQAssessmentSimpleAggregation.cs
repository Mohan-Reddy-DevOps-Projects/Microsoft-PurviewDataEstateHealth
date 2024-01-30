#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;

public class MQAssessmentSimpleAggregation : MQAssessmentAggregationBase
{
    [JsonProperty("type")]
    public override MQAssessmentAggregationType Type
    {
        get => MQAssessmentAggregationType.Simple;
        set { }
    }

    [JsonProperty("aggregationType")]
    [CosmosDBEnumString]
    public required MQAssessmentSimpleAggregationType AggregationType { get; set; }
}

public enum MQAssessmentSimpleAggregationType
{
    Count,
    Sum,
    Average,
    Min,
    Max,
    DistinctCount,
}

