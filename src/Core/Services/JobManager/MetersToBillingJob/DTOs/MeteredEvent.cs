namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
using System;

public class MeteredEvent
{
    public String TenantId { get; set; }
    public string AccountId { get; set; }
    public String DMSScope { get; set; }
}
