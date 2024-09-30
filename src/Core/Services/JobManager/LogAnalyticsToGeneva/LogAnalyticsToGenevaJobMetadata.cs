// -----------------------------------------------------------------------
// <copyright file="LogAnalyticsToGenevaJobMetadata.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Metadata for LogAnalyticsToGeneva Job
/// </summary>
public class LogAnalyticsToGenevaJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Stage Processed
    /// </summary>
    public bool StageProcessed { get; set; }

    /// <summary>
    /// Just informative.
    /// </summary>
    public DateTimeOffset LastPollTime { get; set; }
}
