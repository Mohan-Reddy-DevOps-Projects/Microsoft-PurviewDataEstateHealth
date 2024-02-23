namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

public enum DHComputingJobStatus
{
    Unknown,
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
