#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[EntityWrapper(DHRuleOrGroupBaseWrapperDerivedTypes.Group, EntityCategory.Rule)]
public class DHRuleGroupWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyGroupOperator = "groupOperator";
    private const string keyRules = "rules";

    public DHRuleGroupWrapper() : this(new JObject()) { }

    [EntityProperty(keyGroupOperator)]
    [CosmosDBEnumString]
    public DHRuleGroupOperator GroupOperator
    {
        get => this.GetPropertyValue<DHRuleGroupOperator>(keyGroupOperator);
        set => this.SetPropertyValue(keyGroupOperator, value);
    }

    private IEnumerable<DHRuleBaseWrapper>? rules;

    [EntityProperty(keyRules)]
    public IEnumerable<DHRuleBaseWrapper> Rules
    {
        get => this.rules ??= this.GetPropertyValueAsWrappers<DHRuleBaseWrapper>(keyRules);
        set
        {
            this.SetPropertyValueFromWrappers(keyRules, value);
            this.rules = value;
        }
    }
}
