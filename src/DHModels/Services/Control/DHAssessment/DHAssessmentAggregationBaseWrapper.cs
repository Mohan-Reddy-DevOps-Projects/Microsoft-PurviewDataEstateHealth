namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Assessment)]
public abstract class DHAssessmentAggregationBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    public DHAssessmentAggregationBaseWrapper() : this([]) { }

    public static DHAssessmentAggregationBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHAssessmentAggregationBaseWrapper>(EntityCategory.Assessment, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }
}

public static class DHAssessmentAggregationBaseWrapperDerivedTypes
{
    public const string Simple = "Simple";
    public const string Expression = "Expression";
}
