// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// Health reports data transfer object.
/// </summary>
public class HealthReportsSummary
{
    /// <summary>
    /// Total number of health reports
    /// </summary>
    [ReadOnly(true)]
    public int TotalReportsCount { get; internal set; }

    /// <summary>
    /// Total number of active health reports
    /// </summary>
    [ReadOnly(true)]
    public int ActiveReportsCount { get; internal set; }

    /// <summary>
    /// Total number of draft health reports.
    /// </summary>
    [ReadOnly(true)]
    public int DraftReportsCount { get; internal set; }
}
