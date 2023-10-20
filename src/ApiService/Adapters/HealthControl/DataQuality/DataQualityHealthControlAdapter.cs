// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles data quality control model resource conversions.
/// </summary>
[ModelAdapter(typeof(DataQualityHealthControlModel), typeof(DataQualityHealthControl))]
public class DataQualityHealthControlAdapter : BaseModelAdapter<DataQualityHealthControlModel, DataQualityHealthControl>
{
    /// <inheritdoc />
    public override DataQualityHealthControlModel ToModel(DataQualityHealthControl resource)
    {
        return new DataQualityHealthControlModel
        {
            Properties = new Models.DataQualityHealthControlProperties
            {
                Id = resource.Id,
                Kind = resource.Kind.ToModel(),
                Name = resource.Properties.Name,
                Description = resource.Properties.Description,
                IsCompositeControl = resource.Properties.IsCompositeControl,
                ControlType = resource.Properties.ControlType.ToModel(),
                OwnerContact = new Models.OwnerContact
                {
                    DisplayName = resource.Properties.OwnerContact.DisplayName,
                    ObjectId = resource.Properties.OwnerContact.ObjectId
                },
                CurrentScore = resource.Properties.CurrentScore,
                TargetScore = resource.Properties.TargetScore,
                ScoreUnit = resource.Properties.ScoreUnit,
                HealthStatus = resource.Properties.HealthStatus,
                ControlStatus = resource.Properties.ControlStatus.ToModel(),
                CreatedAt = resource.Properties.CreatedAt,
                StartsAt = resource.Properties.StartsAt,
                EndsAt = resource.Properties.EndsAt,
                TrendUrl = resource.Properties.TrendUrl,
                BusinessDomainsListLink = resource.Properties.BusinessDomainsListLink
            },
        };
    }

    /// <inheritdoc />
    public override DataQualityHealthControl FromModel(DataQualityHealthControlModel model)
    {
        return new DataQualityHealthControl
        {
            Id = model.Properties.Id,
            Kind = model.Properties.Kind.ToDto(),
            Properties = new DataQualityHealthControlProperties
            {
                Name = model.Properties.Name,
                Description = model.Properties.Description,
                IsCompositeControl = model.Properties.IsCompositeControl,
                ControlType = model.Properties.ControlType.ToDto(),
                OwnerContact = new DataTransferObjects.OwnerContact
                {
                    DisplayName = model.Properties.OwnerContact.DisplayName,
                    ObjectId = model.Properties.OwnerContact.ObjectId
                },
                CurrentScore = model.Properties.CurrentScore,
                TargetScore = model.Properties.TargetScore,
                ScoreUnit = model.Properties.ScoreUnit,
                HealthStatus = model.Properties.HealthStatus,
                ControlStatus = model.Properties.ControlStatus.ToDto(),
                CreatedAt = model.Properties.CreatedAt,
                StartsAt = model.Properties.StartsAt,
                EndsAt = model.Properties.EndsAt,
                TrendUrl = model.Properties.TrendUrl,
                BusinessDomainsListLink = model.Properties.BusinessDomainsListLink
            }
        };
    }
}
