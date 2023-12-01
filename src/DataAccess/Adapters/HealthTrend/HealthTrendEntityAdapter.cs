// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Adapters.HealthTrend;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for HealthTrendEntity to HealthTrendModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IHealthTrendModel), typeof(HealthTrendEntity))]
internal class HealthTrendEntityAdapter : BaseModelAdapter<IHealthTrendModel, HealthTrendEntity>
{
    public override HealthTrendEntity FromModel(IHealthTrendModel model)
    {
        return new HealthTrendEntity()
        {
            Kind = model.Kind,
            Description = model.Description,
            Delta = model.Delta,
            TrendValuesList = model.TrendValuesList,
        };
    }

    public override IHealthTrendModel ToModel(HealthTrendEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new HealthTrendModel()
        {
            Kind = entity.Kind,
            Description = entity.Description,
            Delta = entity.Delta,
            TrendValuesList = entity.TrendValuesList,
        };
    }
}
