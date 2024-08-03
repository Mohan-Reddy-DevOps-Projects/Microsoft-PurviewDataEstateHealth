// -----------------------------------------------------------------------
// <copyright file="CBSBillingReceipt.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;

using System;

/// <summary>
/// Represents a CBS Submission Receipt
/// </summary>
internal class CBSBillingReceipt
{
    public DateTime BillingTimestamp { get; set; }
    public string EventId { get; set; }
    public string CorrelationId { get; set; }
    public string BatchId { get; set; }
    public string Service { get; set; }
    public string BillingEvent { get; set; }
    public string Status { get; set; }
    public string StatusDetail { get; set; }
}