// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
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
    [Required]
    [JsonProperty("reportKind")]
    public HealthReportKind ReportKind { get; set; }
}
