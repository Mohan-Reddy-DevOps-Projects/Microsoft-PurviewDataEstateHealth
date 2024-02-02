#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(MQAssessmentAggregationBaseWrapperDerivedTypes.Simple, EntityCategory.Assessment)]
public class MQAssessmentSimpleAggregationWrapper(JObject jObject) : MQAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyAggregationType = "aggregationType";

    public MQAssessmentSimpleAggregationWrapper() : this(new JObject()) { }

    [EntityProperty(keyAggregationType)]
    [CosmosDBEnumString]
    public MQAssessmentSimpleAggregationType AggregationType
    {
        get => this.GetPropertyValue<MQAssessmentSimpleAggregationType>(keyAggregationType);
        set => this.SetPropertyValue(keyAggregationType, value);
    }
}
