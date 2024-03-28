namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHComputingJobStatus
{
    Unknown,
    Created,
    Failed,
    Succeeded,
    InProgress,
    Cancelling,
    Cancelled,
    Queued,
    Accepted,
    Rejected,
    Skipped,
    Deleting
}
