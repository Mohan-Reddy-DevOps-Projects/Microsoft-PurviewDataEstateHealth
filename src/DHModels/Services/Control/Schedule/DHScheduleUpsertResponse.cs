#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using System;

public class DHScheduleUpsertResponse
{
    [JsonProperty("schedulerId")]
    public Guid SchedulerId { get; set; }
}
