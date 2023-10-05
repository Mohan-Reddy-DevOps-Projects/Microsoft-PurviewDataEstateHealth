// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <inheritdoc />
[Component(typeof(IHealthReportComponent), ServiceVersion.V1)]
internal class HealthReportComponent : BaseComponent<IHealthReportListContext>, IHealthReportComponent
{
    public HealthReportComponent(HealthReportContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc />
    public Task<IHealthReportModel<HealthReportProperties>> Get(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
