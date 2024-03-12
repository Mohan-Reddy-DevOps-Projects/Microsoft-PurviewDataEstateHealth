namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHAssessmentTargetEntityType
{
    DataProduct,
    CriticalDataElement,
    BusinessDomain,
}