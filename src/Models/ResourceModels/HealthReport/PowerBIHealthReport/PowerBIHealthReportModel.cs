// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Power BI health report model
/// </summary>
[HealthReportModel(HealthReportKind.PowerBIHealthReport)]
public class PowerBIHealthReportModel : HealthReportModel<PowerBIHealthReportProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PowerBIHealthReportModel"/> class.
    /// </summary>
    public PowerBIHealthReportModel()
        : base()
    {
    }
}
