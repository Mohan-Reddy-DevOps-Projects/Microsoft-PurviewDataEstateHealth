// -----------------------------------------------------------------------
// <copyright file="MetersToBillingJobMetadata.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Metadata for MetersToBilling Job
/// </summary>
public class MetersToBillingJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Stage Processed
    /// </summary>
    public bool StageProcessed { get; set; }

    /// <summary>
    /// Just informative.
    /// </summary>
    public string LastPollTime { get; set; }
}
