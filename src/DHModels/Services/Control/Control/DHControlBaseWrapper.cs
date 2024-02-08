namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.Control)]
public abstract class DHControlBaseWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHControlBaseWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyDescription = "description";
    private const string keyContacts = "contacts";
    private const string keyReserved = "reserved";
    private const string keyStatusPaletteConfig = "statusPaletteConfig";

    public static DHControlBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHControlBaseWrapper>(EntityCategory.Control, EntityWrapperHelper.GetEntityType(jObject), jObject);
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

    [EntityProperty(keyReserved, true)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
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

    public override void OnUpdate(DHControlBaseWrapper existWrapper, string userId)
    {
        base.OnUpdate(existWrapper, userId);

        this.Reserved = existWrapper.Reserved;
    }
}

public static class DHControlBaseWrapperDerivedTypes
{
    public const string Group = "ControlGroup";
    public const string Node = "ControlNode";
}
