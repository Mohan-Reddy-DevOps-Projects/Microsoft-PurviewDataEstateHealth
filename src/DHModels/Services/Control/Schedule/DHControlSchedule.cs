namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class DHControlSchedule
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("frequency")]
    public string? Frequency { get; set; }

    [JsonProperty("interval")]
    public int? Interval { get; set; }

    [JsonProperty("startTime")]
    public DateTime? StartTime { get; set; }

    [JsonProperty("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonProperty("timeZone")]
    public string? TimeZone { get; set; }

    [JsonProperty("schedule")]
    public DHControlScheduleProperties? Schedule { get; set; }
}

public class DHControlScheduleProperties
{
    [JsonProperty("hours")]
    public List<int>? Hours { get; set; }

    [JsonProperty("minutes")]
    public List<int>? Minutes { get; set; }

    [JsonProperty("monthDays")]
    public List<int>? MonthDays { get; set; }

    [JsonProperty("weekDays")]
    public List<string>? WeekDays { get; set; }

    [JsonProperty("monthlyOccurrences")]
    public List<DHControlScheduleMontlyOccurrences>? MonthlyOccurrences { get; set; }
}

public class DHControlScheduleMontlyOccurrences
{
    [JsonProperty("day")]
    public string? Day { get; set; }

    [JsonProperty("occurrence")]
    public int? Occurrence { get; set; }
}
