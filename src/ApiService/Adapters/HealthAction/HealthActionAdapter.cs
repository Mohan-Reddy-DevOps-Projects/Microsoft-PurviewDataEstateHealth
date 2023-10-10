// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Health action model to DTO adapter.
/// </summary>
[ModelAdapter(typeof(IHealthActionModel), typeof(HealthAction))]
public class HealthActionAdapter : BaseModelAdapter<IHealthActionModel, HealthAction>
{
    /// <inheritdoc />
    public override HealthAction FromModel(IHealthActionModel model)
    {
        var targetDetailsListDTO = new List<DataTransferObjects.TargetDetails>();
        foreach (var targetDetail in model.TargetDetailsList)
        {
            targetDetailsListDTO.Add(new DataTransferObjects.TargetDetails
            {
                TargetKind = targetDetail.TargetKind.ToDto(),
                TargetName = targetDetail.TargetName,
                TargetId = targetDetail.TargetId,
                OwnerContact = new DataTransferObjects.OwnerContact
                {
                    DisplayName = targetDetail.OwnerContact.DisplayName,
                    ObjectId = targetDetail.OwnerContact.ObjectId
                }
            });
        }

        return new HealthAction
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            HealthControlName = model.HealthControlName,
            HealthControlState = model.HealthControlState.ToDto(),
            CreatedAt = model.CreatedAt,
            LastRefreshedAt = model.LastRefreshedAt,
            OwnerContact = new DataTransferObjects.OwnerContact
            {
                DisplayName = model.OwnerContact.DisplayName,
                ObjectId = model.OwnerContact.ObjectId
            },
            TargetDetailsList  = targetDetailsListDTO,
        };
    }

    /// <inheritdoc />
    public override IHealthActionModel ToModel(HealthAction healthActionDto)
    {
        var targetDetailsListModel = new List<TargetDetails>();
        foreach (var targetDetail in healthActionDto.TargetDetailsList)
        {
            targetDetailsListModel.Add(new TargetDetails
            {
                TargetKind = targetDetail.TargetKind.ToModel(),
                TargetName = targetDetail.TargetName,
                TargetId = targetDetail.TargetId,
                OwnerContact = new OwnerContact
                {
                    DisplayName = targetDetail.OwnerContact.DisplayName,
                    ObjectId = targetDetail.OwnerContact.ObjectId
                }
            });
        }

        return new HealthActionModel
        {
            Id = healthActionDto.Id,
            Name = healthActionDto.Name,
            Description = healthActionDto.Description,
            HealthControlName = healthActionDto.HealthControlName,
            HealthControlState = healthActionDto.HealthControlState.ToModel(),
            CreatedAt = healthActionDto.CreatedAt,
            LastRefreshedAt = healthActionDto.LastRefreshedAt,
            OwnerContact = new OwnerContact 
            {
                DisplayName = healthActionDto.OwnerContact.DisplayName,
                ObjectId = healthActionDto.OwnerContact.ObjectId
            },
            TargetDetailsList = targetDetailsListModel,
        };
    }
}
