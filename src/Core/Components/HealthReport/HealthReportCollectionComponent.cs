// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Components.HealthReport;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;

/// <inheritdoc />
[Component(typeof(IHealthReportCollectionComponent), ServiceVersion.V1)]
internal class HealthReportCollectionComponent : BaseComponent<IHealthReportListContext>, IHealthReportCollectionComponent
{
    public HealthReportCollectionComponent(IHealthReportListContext context, int version) : base(context, version)
    {
    }

    public IHealthReportComponent ById(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IBatchResults<IHealthReportModel<HealthReportProperties>>> Get(
        string skipToken = null,
        HealthReportKind? reportKind = null)
    {
        throw new NotImplementedException();
    }
}
