// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Health Actions Summary Adapter
/// </summary>
[ModelAdapter(typeof(IHealthActionsSummaryModel), typeof(HealthActionsSummary))]
public class HealthActionsSummaryAdapter : BaseModelAdapter<IHealthActionsSummaryModel, HealthActionsSummary>
{
    /// <inheritdoc />
    public override HealthActionsSummary FromModel(IHealthActionsSummaryModel model)
    {
        return new HealthActionsSummary
        {
            TotalCompletedActionsCount = model.TotalCompletedActionsCount,
            TotalDismissedActionsCount = model.TotalDismissedActionsCount,
            TotalOpenActionsCount = model.TotalOpenActionsCount,
            HealthActionsDefaultTrendLink = model.HealthActionsDefaultTrendLink,
            LastRefreshDate = model.HealthActionsLastRefreshDate
        };
    }

    /// <inheritdoc />
    public override IHealthActionsSummaryModel ToModel(HealthActionsSummary healthActionSummaryDto)
    {
        return new HealthActionsSummaryModel
        {
            TotalCompletedActionsCount = healthActionSummaryDto.TotalCompletedActionsCount,
            TotalDismissedActionsCount = healthActionSummaryDto.TotalDismissedActionsCount,
            TotalOpenActionsCount = healthActionSummaryDto.TotalOpenActionsCount,
            HealthActionsDefaultTrendLink = healthActionSummaryDto.HealthActionsDefaultTrendLink,
            HealthActionsLastRefreshDate = healthActionSummaryDto.LastRefreshDate
        };
    }
}
