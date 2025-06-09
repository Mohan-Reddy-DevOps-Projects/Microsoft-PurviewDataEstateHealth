// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

/// <summary>
/// Metadata for SQL generation workflow operations.
/// </summary>
internal class SqlGenerationJobMetadata
{
    /// <summary>
    /// SQL Generation status.
    /// </summary>
    [JsonProperty]
    public SqlGenerationWorkflowStatus SqlGenerationStatus { get; set; } = SqlGenerationWorkflowStatus.NotStarted;

    [JsonProperty]
    public string ControlsWorkflowJobRunId { get; set; }
} 