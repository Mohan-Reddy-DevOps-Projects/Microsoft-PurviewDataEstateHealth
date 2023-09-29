// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A Legacy health report class.
/// </summary>
public class LegacyHealthReport : HealthReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyHealthReport"/> class.
    /// </summary>
    public LegacyHealthReport()
    {
        this.ReportKind = HealthReportKind.LegacyHealthReport;
        this.Properties = new LegacyHealthReportProperties();
    }

    /// <summary>
    /// Legacy health report properties.
    /// </summary>
    [Required]
    public LegacyHealthReportProperties Properties { get; set; }
}

