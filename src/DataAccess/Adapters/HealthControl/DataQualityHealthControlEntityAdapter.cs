// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataQualityHealthControlEntity to HealthControlModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(DataQualityHealthControlModel), typeof(DataQualityHealthControlEntity))]
internal class DataQualityHealthControlEntityAdapter : BaseModelAdapter<DataQualityHealthControlModel, DataQualityHealthControlEntity>
{
    public override DataQualityHealthControlEntity FromModel(DataQualityHealthControlModel model)
    {
        return new DataQualityHealthControlEntity()
        {
            Name = model.Properties.Name,
            Description = model.Properties.Description,
            Id = model.Properties.Id,
            IsCompositeControl = model.Properties.IsCompositeControl,
            ControlStatus = model.Properties.ControlStatus,
            ControlType = model.Properties.ControlType,
            OwnerContact = model.Properties.OwnerContact,
            CurrentScore = model.Properties.CurrentScore,
            TargetScore = model.Properties.TargetScore,
            ScoreUnit = model.Properties.ScoreUnit,
            HealthStatus = model.Properties.HealthStatus,
            CreatedAt = model.Properties.CreatedAt,
            StartsAt = model.Properties.StartsAt,
            EndsAt = model.Properties.EndsAt,
            TrendUrl = model.Properties.TrendUrl,
            BusinessDomainsListLink = model.Properties.BusinessDomainsListLink,
            Kind = model.Properties.Kind
        };
    }

    public override DataQualityHealthControlModel ToModel(DataQualityHealthControlEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataQualityHealthControlModel()
        {
            Properties = new DataQualityHealthControlProperties()
            {
                Name = entity.Name,
                Description = entity.Description,
                Id = entity.Id,
                IsCompositeControl = entity.IsCompositeControl,
                ControlStatus = entity.ControlStatus,
                ControlType = entity.ControlType,
                OwnerContact = entity.OwnerContact,
                CurrentScore = entity.CurrentScore,
                TargetScore = entity.TargetScore,
                ScoreUnit = entity.ScoreUnit,
                HealthStatus = entity.HealthStatus,
                CreatedAt = entity.CreatedAt,
                StartsAt = entity.StartsAt,
                EndsAt = entity.EndsAt,
                TrendUrl = entity.TrendUrl,
                BusinessDomainsListLink = entity.BusinessDomainsListLink,
                Kind = entity.Kind
            }
        };
    }
}
