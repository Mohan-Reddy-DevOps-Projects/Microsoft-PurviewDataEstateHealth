namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(MQAssessmentAggregationBaseWrapperDerivedTypes.Simple, EntityCategory.Assessment)]
public class MQAssessmentSimpleAggregationWrapper(JObject jObject) : MQAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyAggregationType = "aggregationType";

    public MQAssessmentSimpleAggregationWrapper() : this([]) { }

    [EntityTypeProperty(keyAggregationType)]
    [CosmosDBEnumString]
    public MQAssessmentSimpleAggregationType AggregationType
    {
        get => this.GetTypePropertyValue<MQAssessmentSimpleAggregationType>(keyAggregationType);
        set => this.SetTypePropertyValue(keyAggregationType, value);
    }
}
