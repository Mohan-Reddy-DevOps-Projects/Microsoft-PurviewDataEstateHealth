namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;
using Newtonsoft.Json.Linq;

public class DHControlGlobalSchedulePayloadWrapper(JObject jObject) : DHControlScheduleWrapper(jObject)
{
    private const string keySystemData = "systemData";

    private SystemDataWrapper? systemData;

    [EntityProperty(keySystemData, true)]
    public SystemDataWrapper SystemData
    {
        get => this.systemData ??= this.GetPropertyValueAsWrapper<SystemDataWrapper>(keySystemData);
        set
        {
            this.SetPropertyValueFromWrapper(keySystemData, value);
            this.systemData = value;
        }
    }
}
