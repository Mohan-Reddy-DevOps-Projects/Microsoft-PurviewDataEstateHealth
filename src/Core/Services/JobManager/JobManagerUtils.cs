// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Collections.Generic;

/// <summary>
/// Utility class with helpers for job manager
/// </summary>
internal class JobManagerUtils
{
    static internal Dictionary<string, string> ShimBackgroundJob(BackgroundJob job)
    {
        Dictionary<string, string> entities = [];
        if (job == null)
        {
            return entities;
        }
        entities.Add("JobPartition", job.JobPartition);
        entities.Add("JobId", job.JobId);
        entities.Add("Callback", job.Callback);
        entities.Add("State", job.State.ToString());
        entities.Add("Metadata", job.Metadata);

        entities.Add("CreatedTime", job.CreatedTime.ToString("O"));
        entities.Add("ChangedTime", job.ChangedTime.ToString("O"));
        entities.Add("Retention", job.Retention.HasValue ? job.Retention.Value.ToString() : String.Empty);
        entities.Add("Flags", job.Flags.ToString());

        entities.Add("LastExecutionTime", job.LastExecutionTime.HasValue ? job.LastExecutionTime.Value.ToString("O") : String.Empty);
        entities.Add("LastExecutionStatus", job.LastExecutionStatus.HasValue ? job.LastExecutionStatus.Value.ToString() : String.Empty);
        entities.Add("NextExecutionTime", job.NextExecutionTime.HasValue ? job.NextExecutionTime.Value.ToString("O") : String.Empty);

        entities.Add("RepeatInterval", job.RepeatInterval.ToString());
        entities.Add("RepeatCount", job.RepeatCount.ToString());
        entities.Add("RetryInterval", job.RetryInterval.ToString());
        entities.Add("RetryCount", job.RetryCount.ToString());
        entities.Add("CurrentRepeatCount", job.CurrentRepeatCount.ToString());
        entities.Add("CurrentRetryCount", job.CurrentRetryCount.ToString());
        entities.Add("TotalSucceededCount", job.TotalSucceededCount.ToString());
        entities.Add("TotalCompletedCount", job.TotalCompletedCount.ToString());
        entities.Add("TotalFailedCount", job.TotalFailedCount.ToString());
        entities.Add("TotalFaultedCount", job.TotalFaultedCount.ToString());
        entities.Add("TotalPostponedCount", job.TotalPostponedCount.ToString());
        return entities;
    }
}
