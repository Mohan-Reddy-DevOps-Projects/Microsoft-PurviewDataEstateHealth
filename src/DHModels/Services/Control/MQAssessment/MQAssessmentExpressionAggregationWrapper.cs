#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(MQAssessmentAggregationBaseWrapperDerivedTypes.Expression, EntityCategory.Assessment)]
public class MQAssessmentExpressionAggregationWrapper(JObject jObject) : MQAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyExpression = "expression";

    public MQAssessmentExpressionAggregationWrapper() : this(new JObject()) { }

    [EntityProperty(keyExpression)]
    public string Expression
    {
        get => this.GetPropertyValue<string>(keyExpression);
        set => this.SetPropertyValue(keyExpression, value);
    }
}

