// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class SparkJobUtils
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static bool IsSuccess(SparkBatchJob jobDetails)
    {
        return jobDetails != null && (jobDetails.Result == SparkBatchJobResultType.Succeeded || jobDetails.State == LivyStates.Success);
    }

    public static bool IsFailure(SparkBatchJob jobDetails)
    {
        return jobDetails != null && (jobDetails.Result == SparkBatchJobResultType.Failed || jobDetails.State == LivyStates.Dead || jobDetails.State == LivyStates.Killed);
    }

    public static bool IsJobCompleted(SparkBatchJob jobDetails)
    {
        return IsSuccess(jobDetails) || IsFailure(jobDetails);
    }

    public static string GenerateStatusMessage(string accountId, SparkBatchJob jobDetails, JobExecutionStatus status, string stageName)
    {
        string detail = status switch
        {
            JobExecutionStatus.Succeeded => $"{stageName}|{status} for account: {accountId}.",
            JobExecutionStatus.Completed => $"{stageName}|{status} for account: {accountId} with details: {JsonSerializer.Serialize(jobDetails, jsonOptions)}",
            JobExecutionStatus.Postponed => $"{stageName}|{status} for account: {accountId}.",
            _ => $"Unknown status for {stageName} for account: {accountId}."
        };

        return detail;
    }

    public static JobExecutionStatus DetermineJobStageStatus(SparkBatchJob jobDetails)
    {
        if (IsSuccess(jobDetails))
        {
            return JobExecutionStatus.Succeeded;
        }
        if (IsFailure(jobDetails))
        {
            return JobExecutionStatus.Completed;
        }

        return JobExecutionStatus.Postponed;
    }
}
