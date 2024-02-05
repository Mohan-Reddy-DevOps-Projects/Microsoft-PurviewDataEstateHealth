namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleCreateRequest
{
    [JsonProperty("category")]
    public string Category { get; set; } = "DataHealthSchedule";

    [JsonProperty("callbackRequest")]
    public required DHScheduleCreateRequestCallback CallbackRequest { get; set; }

    [JsonProperty("recurrence")]
    public required DHControlScheduleWrapper Recurrence { get; set; }
}

public class DHScheduleCreateRequestCallback
{
    [JsonProperty("url")]
    public required string Url { get; set; }

    [JsonProperty("method")]
    public string Method { get; set; } = "GET";

    [JsonProperty("body")]
    public object? Body { get; set; }
}
