namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleTriggerRequestPayload
{
    [JsonProperty("scheduleId")]
    public required string ScheduleId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; } = DHScheduleConstant.Category;
}
