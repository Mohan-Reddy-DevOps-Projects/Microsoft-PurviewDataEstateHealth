// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert health control state between model and DTO.
/// </summary>
public static class HealthControlStateAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthControlState ToDto(this HealthControlState model)
    {
        switch (model)
        {
            case HealthControlState.Active:
                return DataTransferObjects.HealthControlState.Active;
            case HealthControlState.Resolved:
                return DataTransferObjects.HealthControlState.Resolved;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthControlState));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthControlState ToModel(this DataTransferObjects.HealthControlState dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthControlState.Active:
                return HealthControlState.Active;
            case DataTransferObjects.HealthControlState.Resolved:
                return HealthControlState.Resolved;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthControlState));
        }
    }
}
