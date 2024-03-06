namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System;

public record DHScoreAggregatedByControlGroup
{
    public required string ControlGroupId { get; set; }
    public required DateTime Time { get; set; }
    public required double Score { get; set; }
}
