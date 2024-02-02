#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Assessment)]
public abstract class MQAssessmentAggregationBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
}

internal static class MQAssessmentAggregationBaseWrapperDerivedTypes
{
    public const string Simple = "Simple";
    public const string Expression = "Expression";
}
