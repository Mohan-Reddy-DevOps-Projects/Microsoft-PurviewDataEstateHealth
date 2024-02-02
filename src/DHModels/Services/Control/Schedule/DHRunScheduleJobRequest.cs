#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHRunScheduleJobRequest
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }
}
