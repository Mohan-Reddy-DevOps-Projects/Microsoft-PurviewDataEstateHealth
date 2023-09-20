// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Power BI Reports Summary Adapter
/// </summary>
[ModelAdapter(typeof(IHealthReportsSummaryModel), typeof(HealthReportsSummary))]
public class HealthReportsSummaryAdapter : BaseModelAdapter<IHealthReportsSummaryModel, HealthReportsSummary>
{
    /// <inheritdoc />
    public override HealthReportsSummary FromModel(IHealthReportsSummaryModel model)
    {
        return new HealthReportsSummary
        {
            TotalReportsCount = model.TotalReportsCount,
            ActiveReportsCount = model.ActiveReportsCount,
            DraftReportsCount = model.DraftReportsCount
        };
    }

    /// <inheritdoc />
    public override IHealthReportsSummaryModel ToModel(HealthReportsSummary healthReportsSummaryDto)
    {
        return new HealthReportsSummaryModel
        {
            TotalReportsCount = healthReportsSummaryDto.TotalReportsCount,
            ActiveReportsCount = healthReportsSummaryDto.ActiveReportsCount,
            DraftReportsCount = healthReportsSummaryDto.DraftReportsCount
        };
    }
}
