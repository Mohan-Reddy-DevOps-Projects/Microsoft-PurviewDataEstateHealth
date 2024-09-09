namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
public class GovernedAssetsMeteredEvent : MeteredEvent
{
    public DateTimeOffset ProcessingTimestamp { get; set; }
    public string JobId { get; set; }
    public long CountOfGovernedAssets { get; set; }
}
