namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleUpsertResponse
{
    [JsonProperty("scheduleId")]
    public required string ScheduleId { get; set; }
}
