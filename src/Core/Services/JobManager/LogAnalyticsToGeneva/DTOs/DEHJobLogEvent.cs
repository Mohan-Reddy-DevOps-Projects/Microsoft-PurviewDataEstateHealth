namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.LogAnalyticsToGenevaJob.DTOs;

using System;

/// <summary>
/// As get from Log Analytics billing function 
/// </summary>
public class DEHJobLogEvent
{
    public DateTimeOffset TimeGenerated { get; set; }
    public string TenantId { get; set; }
    public string AccountId { get; set; }
    public string JobName { get; set; }
    public string JobId { get; set; }
    public string JobStatus { get; set; }
    public string ErrorMessage { get; set; }
}
