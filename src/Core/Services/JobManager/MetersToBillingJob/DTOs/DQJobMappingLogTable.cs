namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
using System;


public class DQJobMappingLogTable
{
    public Guid DQJobId { get; set; }
    public string BatchId { get; set; }
    public Guid TenantId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string JobStatus { get; set; }
    public string ControlId { get; set; }
    public string ControlName { get; set; }
}