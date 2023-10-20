// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

/// <summary>
/// A Health report class.
/// </summary>
[KnownType(typeof(PowerBIHealthReport))]
[KnownType(typeof(LegacyHealthReport))]
[JsonConverter(typeof(HealthReportConverter))]
public abstract partial class HealthReport 
{
    /// <summary>
    /// Kind of report.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public HealthReportKind ReportKind { get; set; }

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
    /// Report status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [ReadOnly(true)]
    public HealthResourceStatus ReportStatus { get; set; }

    /// <summary>
    /// Report type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [ReadOnly(true)]
    public HealthResourceType ReportType { get; set; }

    /// <summary>
    /// Report last refreshed at.
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshedAt { get; internal set; }
}
