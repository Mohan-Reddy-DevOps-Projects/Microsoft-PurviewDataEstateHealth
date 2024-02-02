#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHRuleOrGroupBaseWrapperDerivedTypes.ExpressionRule, EntityCategory.Rule)]
public class DHExpressionRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyExpression = "expression";

    public DHExpressionRuleWrapper() : this(new JObject()) { }

    [EntityProperty(keyExpression)]
    public string Expression
    {
        get => this.GetPropertyValue<string>(keyExpression);
        set => this.SetPropertyValue(keyExpression, value);
    }
}
