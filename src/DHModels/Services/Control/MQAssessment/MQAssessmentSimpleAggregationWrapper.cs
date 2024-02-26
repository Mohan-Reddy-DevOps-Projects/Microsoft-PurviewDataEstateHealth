namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(MQAssessmentAggregationBaseWrapperDerivedTypes.Simple, EntityCategory.Assessment)]
public class MQAssessmentSimpleAggregationWrapper(JObject jObject) : MQAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyAggregationType = "aggregationType";

    public MQAssessmentSimpleAggregationWrapper() : this([]) { }

    [EntityTypeProperty(keyAggregationType)]
    public MQAssessmentSimpleAggregationType? AggregationType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyAggregationType);
            return Enum.TryParse<MQAssessmentSimpleAggregationType>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyAggregationType, value?.ToString());
    }
}
