// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health report status between model and DTO.
/// </summary>
public static class HealthResourceStatusAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthResourceStatus ToDto(this HealthResourceStatus model)
    {
        switch (model)
        {
            case HealthResourceStatus.Draft:
                return DataTransferObjects.HealthResourceStatus.Draft;
            case HealthResourceStatus.Active:
                return DataTransferObjects.HealthResourceStatus.Active;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthResourceStatus));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthResourceStatus ToModel(this DataTransferObjects.HealthResourceStatus dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthResourceStatus.Active:
                return HealthResourceStatus.Active;
            case DataTransferObjects.HealthResourceStatus.Draft:
                return HealthResourceStatus.Draft;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthResourceStatus));
        }
    }
}
