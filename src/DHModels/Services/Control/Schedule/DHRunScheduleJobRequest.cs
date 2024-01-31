#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using System;

public class DHRunScheduleJobRequest
{
    [JsonProperty("controlId")]
    public Guid ControlId { get; set; }
}
