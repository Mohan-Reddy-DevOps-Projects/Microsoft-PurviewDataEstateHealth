// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the Health reports summary model.
/// </summary>
public interface IHealthReportsSummaryModel
{
    /// <summary>
    /// Total number of health reports
    /// </summary>
    int TotalReportsCount { get; }

    /// <summary>
    /// Total number of active health reports
    /// </summary>
    int ActiveReportsCount { get; }

    /// <summary>
    /// Total number of draft health reports.
    /// </summary>
    int DraftReportsCount { get; }
}
