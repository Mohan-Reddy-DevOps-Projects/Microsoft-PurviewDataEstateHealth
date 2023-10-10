// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for HealthActionEntity to HealthActionModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IHealthActionModel), typeof(HealthActionEntity))]
internal class HealthActionEntityAdapter : BaseModelAdapter<IHealthActionModel, HealthActionEntity>
{
    public override HealthActionEntity FromModel(IHealthActionModel model)
    {
        return new HealthActionEntity()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            OwnerContact = model.OwnerContact,
            HealthControlName = model.HealthControlName,
            HealthControlState = model.HealthControlState,
            CreatedAt = model.CreatedAt,
            LastRefreshedAt = model.LastRefreshedAt,
            TargetDetailsList = model.TargetDetailsList
        };
    }

    public override IHealthActionModel ToModel(HealthActionEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new HealthActionModel()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            OwnerContact = entity.OwnerContact,
            HealthControlName = entity.HealthControlName,
            HealthControlState = entity.HealthControlState,
            CreatedAt = entity.CreatedAt,
            LastRefreshedAt = entity.LastRefreshedAt,
            TargetDetailsList = entity.TargetDetailsList
        };
    }
}
