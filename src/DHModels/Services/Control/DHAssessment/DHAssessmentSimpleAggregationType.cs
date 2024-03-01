namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHAssessmentSimpleAggregationType
{
    Count,
    Sum,
    Average,
    Min,
    Max,
    DistinctCount,
}
