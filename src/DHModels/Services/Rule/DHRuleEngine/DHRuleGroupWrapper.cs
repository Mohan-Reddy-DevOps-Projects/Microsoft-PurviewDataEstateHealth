namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.Group, EntityCategory.Rule)]
public class DHRuleGroupWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyGroupOperator = "groupOperator";
    private const string keyRules = "rules";

    public DHRuleGroupWrapper() : this([]) { }

    [EntityTypeProperty(keyGroupOperator)]
    public DHRuleGroupOperator? GroupOperator
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyGroupOperator);
            return Enum.TryParse<DHRuleGroupOperator>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyGroupOperator, value?.ToString());
    }

    private IEnumerable<DHRuleBaseWrapper>? rules;

    [EntityTypeProperty(keyRules)]
    public IEnumerable<DHRuleBaseWrapper> Rules
    {
        get => this.rules ??= this.GetTypePropertyValueAsWrappers<DHRuleBaseWrapper>(keyRules);
        set
        {
            this.SetTypePropertyValueFromWrappers(keyRules, value);
            this.rules = value;
        }
    }
}
