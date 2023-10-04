// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for HealthActionsSummaryEntity to HealthActionsSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IHealthActionsSummaryModel), typeof(HealthActionsSummaryEntity))]
internal class HealthActionsSummaryEntityAdapter : BaseModelAdapter<IHealthActionsSummaryModel, HealthActionsSummaryEntity>
{
    public override HealthActionsSummaryEntity FromModel(IHealthActionsSummaryModel model)
    {
        return new HealthActionsSummaryEntity()
        {
            HealthActionsTrendLink = model.HealthActionsTrendLink,
            HealthActionsLastRefreshDate = model.HealthActionsLastRefreshDate,
            TotalCompletedActionsCount = model.TotalCompletedActionsCount,
            TotalDismissedActionsCount = model.TotalDismissedActionsCount,
            TotalOpenActionsCount = model.TotalOpenActionsCount,
        };
    }

    public override IHealthActionsSummaryModel ToModel(HealthActionsSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new HealthActionsSummaryModel()
        {
           HealthActionsLastRefreshDate = entity.HealthActionsLastRefreshDate,
           HealthActionsTrendLink = entity.HealthActionsTrendLink,
           TotalCompletedActionsCount = entity.TotalCompletedActionsCount,
           TotalDismissedActionsCount = entity.TotalDismissedActionsCount,
           TotalOpenActionsCount = entity.TotalOpenActionsCount,
        };
    }
}
