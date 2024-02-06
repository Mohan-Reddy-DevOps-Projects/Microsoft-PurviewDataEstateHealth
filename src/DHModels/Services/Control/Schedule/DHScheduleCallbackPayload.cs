namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleCallbackPayload
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }
}
