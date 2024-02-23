namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Newtonsoft.Json;
using System;

public class DHControlMDQJobCallbackPayload
{
    [JsonProperty("dqJobId")]
    public required string DQJobId { get; set; }

    [JsonProperty("jobStatus")]
    public required string JobStatus { get; set; }

    public DHComputingJobStatus ParseJobStatus()
    {
        return Enum.TryParse(this.JobStatus, out DHComputingJobStatus status) ? status : DHComputingJobStatus.Unknown;
    }
}
