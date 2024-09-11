// -----------------------------------------------------------------------
// <copyright file="DEHProcessedJobs.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;

using System;

/// <summary>
/// Represents a DEH processed job
/// </summary>
internal class DEHProcessedJobs
{
    public string TenantId { get; set; }
    public string AccountId { get; set; }
    public string MDQBatchId { get; set; }
    public string JobId { get; set; }
    public double MDQJobDuration { get; set; }
    public double MDQProcessingUnits { get; set; }
    public double DEHJobDuration { get; set; }
    public double DEHProcessingUnits { get; set; }

}