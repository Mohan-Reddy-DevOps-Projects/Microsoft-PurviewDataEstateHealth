// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A PowerBI health report class.
/// </summary>
public class PowerBIHealthReport : HealthReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PowerBIHealthReport"/> class.
    /// </summary>
    public PowerBIHealthReport()
    {
        this.ReportKind = HealthReportKind.PowerBIHealthReport;
        this.Properties = new PowerBIHealthReportProperties();
    }

    /// <summary>
    /// PowerBI health report properties.
    /// </summary>
    [Required]
    public PowerBIHealthReportProperties Properties { get; set; }
}

