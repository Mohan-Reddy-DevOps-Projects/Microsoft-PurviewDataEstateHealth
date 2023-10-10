// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for HealthReportsSummaryEntity to HealthReportsSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IHealthReportsSummaryModel), typeof(HealthReportsSummaryEntity))]
internal class HealthReportsSummaryEntityAdapter : BaseModelAdapter<IHealthReportsSummaryModel, HealthReportsSummaryEntity>
{
    public override HealthReportsSummaryEntity FromModel(IHealthReportsSummaryModel model)
    {
        return new HealthReportsSummaryEntity()
        {
            TotalReportsCount = model.TotalReportsCount,
            ActiveReportsCount = model.ActiveReportsCount,
            DraftReportsCount = model.DraftReportsCount
        };
    }

    public override IHealthReportsSummaryModel ToModel(HealthReportsSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new HealthReportsSummaryModel()
        {
           TotalReportsCount = entity.TotalReportsCount,
           ActiveReportsCount = entity.ActiveReportsCount,
           DraftReportsCount = entity.DraftReportsCount
        };
    }
}
