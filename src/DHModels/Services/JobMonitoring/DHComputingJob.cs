#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

using Newtonsoft.Json;
using System;

public class DHComputingJob
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("controlId")]
    public required Guid ControlId { get; set; }

    [JsonProperty("computingJobId")]
    public required Guid ComputingJobId { get; set; }

    [JsonProperty("createTime")]
    public required long CreateTime { get; set; }

    [JsonProperty("startTime")]
    public required long StartTime { get; set; }

    [JsonProperty("endTime")]
    public required long EndTime { get; set; }

    [JsonProperty("status")]
    public required DHComputingJobStatus Status { get; set; }

    [JsonProperty("progress")]
    public double? Progress { get; set; }
}

public enum DHComputingJobStatus
{
    NotStarted,
    Running,
    Succeeded,
    Failed,
    Canceled
}
