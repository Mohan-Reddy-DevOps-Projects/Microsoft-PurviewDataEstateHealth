namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(EntityCategory.Control)]
public abstract class DHControlBaseWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHControlBaseWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyDescription = "description";
    private const string keyContacts = "contacts";
    private const string keySystemTemplate = "systemTemplate";
    private const string keyStatusPaletteConfig = "statusPaletteConfig";
    private const string keyStatus = "status";

    public static DHControlBaseWrapper Create(JObject jObject)
    {
        var entity = EntityWrapperHelper.CreateEntityWrapper<DHControlBaseWrapper>(EntityCategory.Control, EntityWrapperHelper.GetEntityType(jObject), jObject);
        entity.Status ??= DHControlStatus.Enabled;
        return entity;
    }

    public DHControlBaseWrapper() : this([]) { }

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

    [EntityProperty(keySystemTemplate, true)]
    public string SystemTemplate
    {
        get => this.GetPropertyValue<string>(keySystemTemplate);
        set => this.SetPropertyValue(keySystemTemplate, value);
    }

    private DHControlStatusPaletteConfigWrapper? statusPaletteConfig;

    [EntityProperty(keyStatusPaletteConfig)]
    public DHControlStatusPaletteConfigWrapper StatusPaletteConfig
    {
        get => this.statusPaletteConfig ??= this.GetPropertyValueAsWrapper<DHControlStatusPaletteConfigWrapper>(keyStatusPaletteConfig);
        set
        {
            this.SetPropertyValueFromWrapper(keyStatusPaletteConfig, value);
            this.statusPaletteConfig = value;
        }
    }

    [EntityProperty(keyStatus)]
    public DHControlStatus? Status
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyStatus);
            return Enum.TryParse<DHControlStatus>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyStatus, value?.ToString());
    }

    public override void OnUpdate(DHControlBaseWrapper existWrapper, string userId)
    {
        base.OnUpdate(existWrapper, userId);

        this.SystemTemplate = existWrapper.SystemTemplate;
    }
}

public static class DHControlBaseWrapperDerivedTypes
{
    public const string Group = "ControlGroup";
    public const string Node = "ControlNode";
}
