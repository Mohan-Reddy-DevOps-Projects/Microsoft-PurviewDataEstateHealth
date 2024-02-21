namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHScheduleCreateRequestPayload
{
    [JsonProperty("scheduleId")]
    public string? ScheduleId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; } = DHScheduleConstant.Category;

    [JsonProperty("callbackRequest")]
    public required DHScheduleCreateRequestCallback CallbackRequest { get; set; }

    [JsonProperty("recurrence")]
    public JObject? Recurrence { get; private set; }

    public void SetRecurrence(DHControlScheduleWrapper schedule)
    {
        this.Recurrence = schedule.JObject.DeepClone() as JObject;
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
