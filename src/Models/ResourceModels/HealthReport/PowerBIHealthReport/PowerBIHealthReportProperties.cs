// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Properties for PowerBI health report
/// </summary>
public class PowerBIHealthReportProperties : HealthReportProperties
{
    /// <summary>
    /// Dataset Id
    /// </summary>
    [JsonProperty("datasetId")]
    public Guid DatasetId { get; set; }

    /// <summary>
    /// Created At
    /// </summary>
    [JsonProperty("createdAt")]
    public string CreatedAt { get; set; }

    /// <summary>
    /// Created By
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Modified At
    /// </summary>
    [JsonProperty("modifiedAt")]
    public string ModifiedAt { get; set; }

    /// <summary>
    /// Modiifed By
    /// </summary>
    [JsonProperty("modifiedBy")]
    public string ModifiedBy { get; set; }

    /// <summary>
    /// Embed Link
    /// </summary>
    [JsonProperty("embedLink")]
    public string EmbedLink { get; set; }
}
