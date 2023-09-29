// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Legacy health report model
/// </summary>
[HealthReportModel(HealthReportKind.LegacyHealthReport)]
public class LegacyHealthReportModel : HealthReportModel<LegacyHealthReportProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyHealthReportModel"/> class.
    /// </summary>
    public LegacyHealthReportModel()
        : base()
    {
    }
}
