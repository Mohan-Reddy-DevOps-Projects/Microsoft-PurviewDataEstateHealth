namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Assessment)]
public abstract class MQAssessmentAggregationBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    public MQAssessmentAggregationBaseWrapper() : this([]) { }

    public static MQAssessmentAggregationBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<MQAssessmentAggregationBaseWrapper>(EntityCategory.Assessment, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }
}

public static class MQAssessmentAggregationBaseWrapperDerivedTypes
{
    public const string Simple = "Simple";
    public const string Expression = "Expression";
}
