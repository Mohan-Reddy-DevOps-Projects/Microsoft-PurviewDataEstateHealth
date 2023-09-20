// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc/> 
public class HealthReportsSummaryModel : IHealthReportsSummaryModel
{
    /// <summary>
    /// Total number of health reports
    /// </summary>
    [JsonProperty("totalReportsCount")]
    public int TotalReportsCount { get; set; }

    /// <summary>
    /// Total number of active health reports
    /// </summary>
    [JsonProperty("activeReportsCount")]
    public int ActiveReportsCount { get; set; }

    /// <summary>
    /// Total number of health reports.
    /// </summary>
    [JsonProperty("draftReportsCount")]
    public int DraftReportsCount { get; set; }
}
