namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

public class DHControlScheduleStoragePayloadWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHControlScheduleStoragePayloadWrapper>(jObject)
{
    public static DHControlScheduleStoragePayloadWrapper Create(JObject jObject)
    {
        return new DHControlScheduleStoragePayloadWrapper(jObject);
    }

    private const string keyType = "type";
    private const string keyProperties = "properties";

    [EntityProperty(keyType)]
    public string Type
    {
        get => this.GetPropertyValue<string>(keyType);
        set => this.SetPropertyValue(keyType, value);
    }

    private DHControlScheduleWrapper? properties;
    [EntityProperty(keyProperties)]
    public DHControlScheduleWrapper Properties
    {
        get => this.properties ??= this.GetPropertyValueAsWrapper<DHControlScheduleWrapper>(keyProperties);
        set
        {
            this.SetPropertyValueFromWrapper(keyProperties, value);
            this.properties = value;
        }
    }

    public override void OnCreate(string userId)
    {
        base.OnCreate(userId);

    }
}

public static class DHControlScheduleType
{
    public const string ControlGlobal = "ControlGlobal";
    public const string ControlNode = "ControlNode";
}
