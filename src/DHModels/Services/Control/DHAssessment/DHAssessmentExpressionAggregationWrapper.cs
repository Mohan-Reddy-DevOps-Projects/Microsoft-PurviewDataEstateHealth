namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHAssessmentAggregationBaseWrapperDerivedTypes.Expression, EntityCategory.Assessment)]
public class DHAssessmentExpressionAggregationWrapper(JObject jObject) : DHAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyExpression = "expression";

    public DHAssessmentExpressionAggregationWrapper() : this([]) { }

    [EntityTypeProperty(keyExpression)]
    public string Expression
    {
        get => this.GetTypePropertyValue<string>(keyExpression);
        set => this.SetTypePropertyValue(keyExpression, value);
    }
}

