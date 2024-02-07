namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.ExpressionRule, EntityCategory.Rule)]
public class DHExpressionRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyExpression = "expression";
    private const string keyCheckPoint = "checkPoint";

    public DHExpressionRuleWrapper() : this([]) { }

    [EntityTypeProperty(keyCheckPoint)]
    public DHCheckPoints CheckPoint
    {
        get => this.GetTypePropertyValue<DHCheckPoints>(keyCheckPoint);
        set => this.SetTypePropertyValue(keyCheckPoint, value);
    }

    [EntityTypeProperty(keyExpression)]
    public string Expression
    {
        get => this.GetTypePropertyValue<string>(keyExpression);
        set => this.SetTypePropertyValue(keyExpression, value);
    }
}
