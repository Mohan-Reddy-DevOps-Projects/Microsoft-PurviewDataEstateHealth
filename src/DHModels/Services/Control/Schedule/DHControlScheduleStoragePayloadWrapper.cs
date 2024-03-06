namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json.Serialization;

public class DHControlScheduleStoragePayloadWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHControlScheduleStoragePayloadWrapper>(jObject)
{
    public static DHControlScheduleStoragePayloadWrapper Create(JObject jObject)
    {
        return new DHControlScheduleStoragePayloadWrapper(jObject);
    }

    private const string keyType = "type";
    private const string keyProperties = "properties";

    [EntityProperty(keyType)]
    [EntityRequiredValidator]
    public DHControlScheduleType? Type
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyType);
            return Enum.TryParse<DHControlScheduleType>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyType, value.ToString());
    }

    private DHControlScheduleWrapper? properties;
    [EntityProperty(keyProperties)]
    [EntityRequiredValidator]
    public DHControlScheduleWrapper Properties
    {
        get => this.properties ??= this.GetPropertyValueAsWrapper<DHControlScheduleWrapper>(keyProperties);
        set
        {
            this.SetPropertyValueFromWrapper(keyProperties, value);
            this.properties = value;
        }
    }

    public override void OnCreate(string userId, string? id = null)
    {
        base.OnCreate(userId, id);
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DHControlScheduleType
{
    ControlGlobal,
    ControlNode,
}
