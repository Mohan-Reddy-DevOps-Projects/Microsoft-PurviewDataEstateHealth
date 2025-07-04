﻿namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;

using System;

/// <summary>
/// As get from Log Analytics billing function 
/// </summary>
public class DEHMeteredEvent : MeteredEvent
{
    public DateTimeOffset ProcessingTimestamp { get; set; }
    public string ApplicationId { get; set; }
    public string ProcessingTier { get; set; }
    public string EventCorrelationId { get; set; }
    public string MDQBatchId { get; set; }
    public double JobDuration { get; set; }
    public string BusinessDomainId { get; set; }
    public string JobStatus { get; set; }
    public double ProcessingUnits { get; set; }
    public DateTimeOffset JobStartTime { get; set; }
    public DateTimeOffset JobEndTime { get; set; }
    public String DMSJobSubType { get; set; }
    public String JobId { get; set; }
    public String ClientTenantId { get; set; }
    public string ReportingDimensions { get; set; }
    public string CorrelationId { get; set; }
}
