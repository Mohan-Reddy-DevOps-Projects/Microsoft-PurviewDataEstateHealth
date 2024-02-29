namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleCallbackPayload
{
    [JsonProperty("controlId")]
    public string? ControlId { get; set; }
}
