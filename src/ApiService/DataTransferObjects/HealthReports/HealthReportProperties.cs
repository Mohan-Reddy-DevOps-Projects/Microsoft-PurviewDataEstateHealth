// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.ComponentModel;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// A Health report property bag.
/// </summary>
public class HealthReportProperties
{
    /// <summary>
    /// Report id.
    /// </summary>
    [ReadOnly(true)]
    public Guid Id { get; internal set; }

    /// <summary>
    /// Report name.
    /// </summary>
    [ReadOnly(true)]
    public string Name { get; set; }

    /// <summary>
    /// Report category.
    /// </summary>
    [ReadOnly(true)]
    public string Category { get; set; }

    /// <summary>
    /// Report description.
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; set; }

    /// <summary>
    /// Report status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [ReadOnly(true)]
    public HealthReportStatus ReportStatus { get; set; }

    /// <summary>
    /// Report type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [ReadOnly(true)]
    public HealthReportType ReportType { get; set; }

    /// <summary>
    /// Report last refreshed at.
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshedAt { get; internal set; }
}
