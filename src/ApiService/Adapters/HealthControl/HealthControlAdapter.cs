// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles DataGovernance control model resource conversions.
/// </summary>
[ModelAdapter(typeof(HealthControlModel), typeof(HealthControl))]
public class HealthControlAdapter : BaseModelAdapter<HealthControlModel, HealthControl>
{
    /// <inheritdoc />
    public override HealthControlModel ToModel(HealthControl resource)
    {
        return new HealthControlModel
        {
            Id = resource.Id,
            Kind = resource.Kind.ToModel(),
            Name = resource.Name,
            Description = resource.Description,
            IsCompositeControl = resource.IsCompositeControl,
            ControlType = resource.ControlType.ToModel(),
            OwnerContact = new Models.OwnerContact
            {
                DisplayName = resource.OwnerContact.DisplayName,
                ObjectId = resource.OwnerContact.ObjectId
            },
            CurrentScore = resource.CurrentScore,
            TargetScore = resource.TargetScore,
            ScoreUnit = resource.ScoreUnit,
            HealthStatus = resource.HealthStatus,
            ControlStatus = resource.ControlStatus.ToModel(),
            CreatedAt = resource.CreatedAt,
            StartsAt = resource.StartsAt,
            EndsAt = resource.EndsAt,
            TrendUrl = resource.TrendUrl,
            ParentControlId = resource.ParentControlId,
            LastRefreshedAt = resource.LastRefreshedAt,
            ModifiedAt = resource.ModifiedAt
        };
    }

    /// <inheritdoc />
    public override HealthControl FromModel(HealthControlModel model)
    {
        return new HealthControl
        {
            Id = model.Id,
            Kind = model.Kind.ToDto(),
            Name = model.Name,
            Description = model.Description,
            IsCompositeControl = model.IsCompositeControl,
            ControlType = model.ControlType.ToDto(),
            OwnerContact = new DataTransferObjects.OwnerContact
            {
                DisplayName = model.OwnerContact.DisplayName,
                ObjectId = model.OwnerContact.ObjectId
            },
            CurrentScore = model.CurrentScore,
            TargetScore = model.TargetScore,
            ScoreUnit = model.ScoreUnit,
            HealthStatus = model.HealthStatus,
            ControlStatus = model.ControlStatus.ToDto(),
            CreatedAt = model.CreatedAt,
            StartsAt = model.StartsAt,
            EndsAt = model.EndsAt,
            TrendUrl = model.TrendUrl,
        };
    }
}
