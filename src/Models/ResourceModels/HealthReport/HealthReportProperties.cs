// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Health Report property bag that should be persisted to metadata store.
/// </summary>
[JsonConverter(typeof(HealthReportPropertiesConverter))]
public class HealthReportProperties
{
    /// <summary>
    /// Report id.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Report name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Report category.
    /// </summary>
    [JsonProperty("category")]
    public string Category { get; set; }

    /// <summary>
    /// Report status.
    /// </summary>
    [JsonProperty("reportStatus")]
    public HealthResourceStatus ReportStatus { get; set; }

    /// <summary>
    /// Report type.
    /// </summary>
    [JsonProperty("reportType")]
    public HealthResourceType ReportType { get; set; }

    /// <summary>
    /// Report kind.
    /// </summary>
    [JsonProperty("reportKind")]
    public HealthReportKind ReportKind { get; set; }
}
