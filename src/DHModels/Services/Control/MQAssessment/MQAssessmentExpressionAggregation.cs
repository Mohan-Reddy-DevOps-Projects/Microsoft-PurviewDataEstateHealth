#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Newtonsoft.Json;

public class MQAssessmentExpressionAggregation : MQAssessmentAggregationBase
{
    [JsonProperty("type")]
    public override MQAssessmentAggregationType Type
    {
        get => MQAssessmentAggregationType.Expression;
        set { }
    }

    [JsonProperty("expression")]
    public required string Expression { get; set; }
}

