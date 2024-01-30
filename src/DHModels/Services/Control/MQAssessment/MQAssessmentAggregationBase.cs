#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;

public abstract class MQAssessmentAggregationBase
{
    [JsonProperty("type")]
    [CosmosDBEnumString]
    public abstract MQAssessmentAggregationType Type { get; set; }
}

public enum MQAssessmentAggregationType
{
    Simple,
    Expression,
}