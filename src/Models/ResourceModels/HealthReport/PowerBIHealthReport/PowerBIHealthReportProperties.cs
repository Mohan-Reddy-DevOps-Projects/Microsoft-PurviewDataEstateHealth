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
    /// Embed Link
    /// </summary>
    [JsonProperty("embedLink")]
    public string EmbedLink { get; set; }

    /// <summary>
    /// Report description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }
}
