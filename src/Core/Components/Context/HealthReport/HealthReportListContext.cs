// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;

internal class HealthReportListContext : IHealthReportListContext
{
    public ServiceVersion Version { get; set; }

    public Guid AccountId { get; set; }

    public string Location { get; set; }

    public Guid TenantId { get; set; }

    public object Key => throw new NotImplementedException();
}
