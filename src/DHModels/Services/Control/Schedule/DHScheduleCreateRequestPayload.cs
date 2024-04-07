namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHScheduleCreateRequestPayload
{
    [JsonProperty("scheduleId")]
    public string? ScheduleId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; } = DHScheduleConstant.Category;

    [JsonProperty("state")]
    [JsonConverter(typeof(StringEnumConverter))]
    public required DHScheduleState State { get; set; }

    [JsonProperty("callbackRequest")]
    public required DHScheduleCreateRequestCallback CallbackRequest { get; set; }

    [JsonProperty("recurrence")]
    public JObject? Recurrence { get; private set; }

    public void SetRecurrence(DHControlScheduleWrapper schedule)
    {
        this.Recurrence = schedule.JObject.DeepClone() as JObject;
        var startTime = schedule.StartTime;
        var endTime = schedule.EndTime;
        if (startTime.HasValue && this.Recurrence != null)
        {
            this.Recurrence["startTime"] = startTime.Value.GetDateTimeStr();
        }
        if (endTime.HasValue && this.Recurrence != null)
        {
            this.Recurrence["endTime"] = endTime.Value.GetDateTimeStr();
        }
    }
}

public class DHScheduleCreateRequestCallback
{
    [JsonProperty("url")]
    public required string Url { get; set; }

    [JsonProperty("method")]
    public required string Method { get; set; }

    [JsonProperty("body")]
    public required DHScheduleCallbackPayload Body { get; set; }

    [JsonProperty("headers")]
    public Dictionary<string, string>? Headers { get; set; }
}

public enum DHScheduleState
{
    Disabled,
    Enabled
}
