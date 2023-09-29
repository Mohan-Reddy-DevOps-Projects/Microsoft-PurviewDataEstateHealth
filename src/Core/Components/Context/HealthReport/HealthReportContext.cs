// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <inheritdoc cref="IHealthReportContext" />
internal class HealthReportContext :  HealthReportListContext
{
    public Guid ReportId { get; set; }
}
