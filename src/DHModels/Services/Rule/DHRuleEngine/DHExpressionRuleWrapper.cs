namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.ExpressionRule, EntityCategory.Rule)]
public class DHExpressionRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyExpression = "expression";
    private const string keyCheckPoint = "checkPoint";

    public DHExpressionRuleWrapper() : this([]) { }

    [EntityTypeProperty(keyCheckPoint)]
    public DHCheckPoint? CheckPoint
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyCheckPoint);
            return Enum.TryParse<DHCheckPoint>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyCheckPoint, value?.ToString());
    }

    [EntityTypeProperty(keyExpression)]
    public string Expression
    {
        get => this.GetTypePropertyValue<string>(keyExpression);
        set => this.SetTypePropertyValue(keyExpression, value);
    }
}
