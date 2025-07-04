﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Newtonsoft.Json;

internal class DataPlaneSparkJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Purview account model.
    /// </summary>
    [JsonProperty]
    public AccountServiceModel AccountServiceModel { get; set; }

    /// <summary>
    /// Spark pool id.
    /// </summary>
    [JsonProperty]
    public string SparkPoolId { get; set; }

    /// <summary>
    /// Catalog Spark job id.
    /// </summary>
    [JsonProperty]
    public string CatalogSparkJobBatchId { get; set; }

    /// <summary>
    /// Catalog Spark job status.
    /// </summary>
    [JsonProperty]
    public DataPlaneSparkJobStatus CatalogSparkJobStatus { get; set; }

    /// <summary>
    /// Dimension Spark job id.
    /// </summary>
    [JsonProperty]
    public string DimensionSparkJobBatchId { get; set; }

    /// <summary>
    /// Fabric Spark job id.
    /// </summary>
    [JsonProperty]
    public string FabricSparkJobBatchId { get; set; }

    /// <summary>
    /// Dimension Spark job status.
    /// </summary>
    [JsonProperty]
    public DataPlaneSparkJobStatus DimensionSparkJobStatus { get; set; }

    /// <summary>
    /// Dimension Spark job status.
    /// </summary>
    [JsonProperty]
    public DataPlaneSparkJobStatus FabricSparkJobStatus { get; set; }



    /// <summary>
    /// Current schedule start time. 
    /// </summary>
    [JsonProperty]
    public DateTime? CurrentScheduleStartTime { get; set; }
}

public enum DataPlaneSparkJobStatus
{
    Succeeded,
    Failed,
    Others,
}
