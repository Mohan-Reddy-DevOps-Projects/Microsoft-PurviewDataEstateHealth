// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

/// <summary>
/// Metadata for actions generation workflow operations.
/// </summary>
internal class ActionsGenerationJobMetadata
{
    /// <summary>
    /// Actions Generation Workflow status.
    /// </summary>
    [JsonProperty]
    public ActionsGenerationWorkflowStatus ActionsGenerationWorkflowStatus { get; set; } = ActionsGenerationWorkflowStatus.NotStarted;
} 