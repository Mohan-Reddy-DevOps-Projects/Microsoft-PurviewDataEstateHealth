// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health report type between model and DTO.
/// </summary>
public static class HealthResourceTypeAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthResourceType ToDto(this HealthResourceType model)
    {
        switch (model)
        {
            case HealthResourceType.System:
                return DataTransferObjects.HealthResourceType.System;
            case HealthResourceType.Custom:
                return DataTransferObjects.HealthResourceType.Custom;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthResourceType));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthResourceType ToModel(this DataTransferObjects.HealthResourceType dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthResourceType.System:
                return HealthResourceType.System;
            case DataTransferObjects.HealthResourceType.Custom:
                return HealthResourceType.Custom;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthResourceType));
        }
    }
}
