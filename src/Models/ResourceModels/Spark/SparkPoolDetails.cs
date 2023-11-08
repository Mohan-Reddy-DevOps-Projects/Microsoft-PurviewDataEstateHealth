// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

using Newtonsoft.Json;

/// <summary>
/// Spark pool details.
/// </summary>
public class SparkPoolDetails
{
    /// <summary>
    /// Gets or sets the pool type.
    /// </summary>
    [JsonProperty("poolType")]
    public SparkPoolType PoolType { get; set; }
}
