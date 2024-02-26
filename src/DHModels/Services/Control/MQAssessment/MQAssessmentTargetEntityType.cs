namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum MQAssessmentTargetEntityType
{
    DataProduct
}

