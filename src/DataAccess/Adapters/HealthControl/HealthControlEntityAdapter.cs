// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for HealthControlEntity to HealthControlModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(HealthControlModel), typeof(HealthControlEntity))]
internal class HealthControlEntityAdapter : BaseModelAdapter<HealthControlModel, HealthControlEntity>
{
    public override HealthControlEntity FromModel(HealthControlModel model)
    {
        return new HealthControlEntity()
        {
            CurrentScore = model.CurrentScore,
            LastRefreshedAt = model.LastRefreshedAt,
            ControlStatus = model.ControlStatus,
            ControlType = model.ControlType,
            CreatedAt = model.CreatedAt,
            Description = model.Description,
            EndsAt = model.EndsAt,
            HealthStatus = model.HealthStatus,
            IsCompositeControl = model.IsCompositeControl,
            ModifiedAt = model.ModifiedAt,
            Name = model.Name,
            ObjectId = model.Id,
            OwnerContact = model.OwnerContact,
            ParentControlId = model.ParentControlId,
            ScoreUnit = model.ScoreUnit,
            StartsAt = model.StartsAt,
            TargetScore = model.TargetScore,
            TrendUrl = model.TrendUrl,
            HealthControlKind = model.Kind,
            Version = null,
        };
    }

    public override HealthControlModel ToModel(HealthControlEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new HealthControlModel()
        {
            Kind = entity.HealthControlKind,
            CurrentScore = entity.CurrentScore,
            LastRefreshedAt = entity.LastRefreshedAt,
            Id = entity.ObjectId,
            ControlStatus = entity.ControlStatus,
            ControlType = entity.ControlType,
            CreatedAt = entity.CreatedAt,
            Description = entity.Description,
            EndsAt = entity.EndsAt,
            HealthStatus = entity.HealthStatus,
            IsCompositeControl = entity.IsCompositeControl,
            Name = entity.Name,
            OwnerContact = entity.OwnerContact,
            ScoreUnit = entity.ScoreUnit,
            StartsAt = entity.StartsAt,
            TargetScore = entity.TargetScore,
            TrendUrl = entity.TrendUrl,
            ModifiedAt = entity.ModifiedAt,
            ParentControlId = entity.ParentControlId,
        };
    }
}
