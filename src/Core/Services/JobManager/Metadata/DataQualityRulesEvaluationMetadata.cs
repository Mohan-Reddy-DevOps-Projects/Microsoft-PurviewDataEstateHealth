// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

/// <summary>
/// Metadata for controls workflow operations.
/// </summary>
internal class DataQualityRulesEvaluationMetadata
{
    /// <summary>
    /// Controls Workflow Spark job id.
    /// </summary>
    [JsonProperty]
    public string DqRulesEvaluationSparkJobBatchId { get; set; }

    /// <summary>
    /// Controls Workflow status.
    /// </summary>
    [JsonProperty]
    public ControlsWorkflowStatus DqRulesEvaluationSparkJobStatus { get; set; } = ControlsWorkflowStatus.NotStarted;
} 