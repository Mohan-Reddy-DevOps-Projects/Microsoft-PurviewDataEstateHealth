﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Newtonsoft.Json;

internal class SparkJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Purview account model.
    /// </summary>
    [JsonProperty]
    public AccountServiceModel AccountServiceModel { get; set; }

    /// <summary>
    /// Spark job id.
    /// </summary>
    [JsonProperty]
    public string SparkJobBatchId { get; set; }

    /// <summary>
    /// Spark pool id.
    /// </summary>
    [JsonProperty]
    public string SparkPoolId { get; set; }

    /// <summary>
    /// Spark job result. 
    /// </summary>
    [JsonProperty]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Current schedule start time. 
    /// </summary>
    [JsonProperty]
    public DateTime? CurrentScheduleStartTime { get; set; }
}
