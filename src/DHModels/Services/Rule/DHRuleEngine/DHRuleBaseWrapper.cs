namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(EntityCategory.Rule)]
public abstract class DHRuleBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    private const string keyName = "name";
    private const string keySeverity = "severity";
    private const string keyRecommendation = "recommendation";
    private const string keyReserved = "reserved";

    public static DHRuleBaseWrapper Create(JObject jObject)
    {
        var entity = EntityWrapperHelper.CreateEntityWrapper<DHRuleBaseWrapper>(EntityCategory.Rule, EntityWrapperHelper.GetEntityType(jObject), jObject);
        if (entity.Severity == null)
        {
            entity.Severity = DataHealthActionSeverity.Medium;
        }
        return entity;
    }

    [EntityProperty(keyName)]
    [EntityRequiredValidator]
    [EntityNameValidator]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keySeverity)]
    public DataHealthActionSeverity? Severity
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keySeverity);
            return Enum.TryParse<DataHealthActionSeverity>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keySeverity, value?.ToString());
    }

    [EntityProperty(keyRecommendation)]
    public string Recommendation
    {
        get => this.GetPropertyValue<string>(keyRecommendation);
        set => this.SetPropertyValue(keyRecommendation, value);
    }

    [EntityProperty(keyReserved)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }
}

public static class DHRuleBaseWrapperDerivedTypes
{
    public const string SimpleRule = "SimpleRule";
    public const string ExpressionRule = "ExpressionRule";
    public const string Group = "RuleGroup";
}

