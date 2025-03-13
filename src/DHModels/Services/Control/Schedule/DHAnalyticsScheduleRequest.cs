namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class DHAnalyticsScheduleRequest
{
    [JsonProperty("frequency", NullValueHandling = NullValueHandling.Ignore)]
    public string? Frequency { get; set; }

    [JsonProperty("interval", NullValueHandling = NullValueHandling.Ignore)]
    public int? Interval { get; set; }

    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public string? Status { get; set; }

    [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? StartTime { get; set; }

    [JsonProperty("systemData", NullValueHandling = NullValueHandling.Ignore)]
    public SystemData? SystemData { get; set; }

    [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
    public string? TimeZone { get; set; }

}
public class SystemData
{
    [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
    public Guid? CreatedBy { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? CreatedAt { get; set; }
}

