namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System;

public record DHScoreAggregatedByControl
{
    public required string ControlId { get; set; }
    public required string ComputingJobId { get; set; }
    public required string ScheduleRunId { get; set; }
    public required DateTime Time { get; set; }
    public required double Score { get; set; }
}
