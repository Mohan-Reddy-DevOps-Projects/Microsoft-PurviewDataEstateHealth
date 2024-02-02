#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[CosmosDBContainer("DHControl")]
[EntityWrapper(EntityCategory.Control)]
public abstract class DHControlBaseWrapper(JObject jObject) : ContainerEntityBaseWrapper(jObject)
{
    private const string keyName = "name";
    private const string keyDescription = "description";
    private const string keyContacts = "contacts";
    private const string keyReserved = "reserved";
    private const string keyFallbackStatusPaletteId = "fallbackStatusPaletteId";
    private const string keyStatusPaletteRules = "statusPaletteRules";

    public static DHControlBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHControlBaseWrapper>(EntityCategory.Control, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }

    [EntityProperty(keyName)]
    [EntityRequiredValidator]
    [EntityNameValidator]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyDescription)]
    public string Description
    {
        get => this.GetPropertyValue<string>(keyDescription);
        set => this.SetPropertyValue(keyDescription, value);
    }

    private ContactWrapper? contacts;

    [EntityProperty(keyContacts)]
    public ContactWrapper Contacts
    {
        get
        {
            this.contacts ??= this.GetPropertyValueAsWrapper<ContactWrapper>(keyContacts);
            return this.contacts;
        }

        set
        {
            this.SetPropertyValueFromWrapper(keyContacts, value);
            this.contacts = value;
        }
    }

    [EntityProperty(keyReserved)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }

    [EntityProperty(keyFallbackStatusPaletteId)]
    public string FallbackStatusPaletteId
    {
        get => this.GetPropertyValue<string>(keyFallbackStatusPaletteId);
        set => this.SetPropertyValue(keyFallbackStatusPaletteId, value);
    }

    private IEnumerable<DHRuleOrGroupBaseWrapper>? statusPaletteRules;

    [EntityProperty(keyStatusPaletteRules)]
    public IEnumerable<DHRuleOrGroupBaseWrapper> StatusPaletteRules
    {
        get => this.statusPaletteRules ??= this.GetPropertyValueAsWrappers<DHRuleOrGroupBaseWrapper>(keyStatusPaletteRules);
        set
        {
            this.SetPropertyValueFromWrappers(keyStatusPaletteRules, value);
            this.statusPaletteRules = value;
        }
    }
}

internal static class DHControlBaseWrapperDerivedTypes
{
    public const string Group = "Group";
    public const string Node = "Node";
}
