// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

internal sealed class DehScheduleJobMetadata : StagedWorkerJobMetadata
{
    public string ScheduleTenantId { get; set; }

    public string ScheduleAccountId { get; set; }

    public DataPlaneSparkJobMetadata CatalogSparkJobMetadata { get; set; }

    /// <summary>
    /// Metadata for SQL generation workflow operations.
    /// </summary>
    [JsonProperty]
    public SqlGenerationJobMetadata SqlGenerationMetadata { get; set; } = new SqlGenerationJobMetadata();

    /// <summary>
    /// Metadata for controls workflow operations.
    /// </summary>
    [JsonProperty]
    public DataQualityRulesEvaluationMetadata ControlsWorkflowMetadata { get; set; } = new DataQualityRulesEvaluationMetadata();

    /// <summary>
    /// Metadata for actions generation workflow operations.
    /// </summary>
    [JsonProperty]
    public ActionsGenerationJobMetadata ActionsGenerationMetadata { get; set; } = new ActionsGenerationJobMetadata();
}
