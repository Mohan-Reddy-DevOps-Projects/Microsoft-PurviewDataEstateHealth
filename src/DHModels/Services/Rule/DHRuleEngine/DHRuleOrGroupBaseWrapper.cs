#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Rule)]
public abstract class DHRuleOrGroupBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject), IWithId
{
    private const string keyId = "id";
    private const string keyName = "name";
    private const string keyAdditionalProperties = "additionalProperties";
    private const string keyReserved = "reserved";

    public static DHRuleOrGroupBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHRuleOrGroupBaseWrapper>(EntityCategory.Rule, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }

    [EntityProperty(keyId, true)]
    [EntityIdValidator]
    public string Id
    {
        get => this.GetPropertyValue<string>(keyId);
        set => this.SetPropertyValue(keyId, value);
    }

    [EntityProperty(keyName)]
    [EntityRequiredValidator]
    [EntityNameValidator]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyAdditionalProperties)]
    public JObject? AdditionalProperties
    {
        get => this.GetPropertyValue<JObject>(keyAdditionalProperties);
        set => this.SetPropertyValue(keyAdditionalProperties, value);
    }

    [EntityProperty(keyReserved)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }
}

internal static class DHRuleOrGroupBaseWrapperDerivedTypes
{
    public const string SimpleRule = "SimpleRule";
    public const string ExpressionRule = "ExpressionRule";
    public const string Group = "Group";
}

