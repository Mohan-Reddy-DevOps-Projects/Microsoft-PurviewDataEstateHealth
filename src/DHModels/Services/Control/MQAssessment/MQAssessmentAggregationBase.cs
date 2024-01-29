#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Newtonsoft.Json;

public abstract class MQAssessmentAggregationBase
{
    [JsonProperty("type")]
    public abstract MQAssessmentAggregationType Type { get; set; }
}

public enum MQAssessmentAggregationType
{
    Simple,
    Expression,
}