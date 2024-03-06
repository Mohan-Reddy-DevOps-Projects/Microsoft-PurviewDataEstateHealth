namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Rule)]
public abstract class DHRuleBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    public static DHRuleBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHRuleBaseWrapper>(EntityCategory.Rule, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }

    public void ValidateCheckPoints(DHRuleSourceType sourceType)
    {
        RuleValidator.Validate(this, sourceType);
    }
}

public static class DHRuleBaseWrapperDerivedTypes
{
    public const string SimpleRule = "SimpleRule";
    public const string ExpressionRule = "ExpressionRule";
    public const string Group = "RuleGroup";
}
