namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.ExpressionRule, EntityCategory.Rule)]
public class DHExpressionRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyExpression = "expression";
    private const string keyCheckPoint = "checkPoint";

    public DHExpressionRuleWrapper() : this([]) { }

    [EntityProperty(keyCheckPoint)]
    [CosmosDBEnumString]
    public DHCheckPoints CheckPoint
    {
        get => this.GetPropertyValue<DHCheckPoints>(keyCheckPoint);
        set => this.SetPropertyValue(keyCheckPoint, value);
    }

    [EntityProperty(keyExpression)]
    public string Expression
    {
        get => this.GetPropertyValue<string>(keyExpression);
        set => this.SetPropertyValue(keyExpression, value);
    }
}
