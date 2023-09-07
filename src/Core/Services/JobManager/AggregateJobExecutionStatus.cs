// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Concurrent;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// Represents Background Jobs with their single status representing the statuses of all
/// </summary>
public class AggregateJobExecutionStatus
{
    /// <summary>
    /// Dictionary of background job ids mapped to their statuses
    /// </summary>
    public ConcurrentDictionary<string, JobExecutionStatus?> JobsStatuses { get; set; }

    /// <summary>
    /// A single status representing the statuses of all
    /// </summary>
    public JobExecutionStatus JobExecutionStatus { get; set; }
}
