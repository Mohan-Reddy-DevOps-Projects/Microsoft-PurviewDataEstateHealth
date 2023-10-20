// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health control kind between model and DTO.
/// </summary>
public static class HealthControlKindAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthControlKind ToDto(this HealthControlKind model)
    {
        switch (model)
        {
            case HealthControlKind.DataQuality:
                return DataTransferObjects.HealthControlKind.DataQuality;
            case HealthControlKind.DataGovernance:
                return DataTransferObjects.HealthControlKind.DataGovernance;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthControlKind));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthControlKind ToModel(this DataTransferObjects.HealthControlKind dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthControlKind.DataQuality:
                return HealthControlKind.DataQuality;
            case DataTransferObjects.HealthControlKind.DataGovernance:
                return HealthControlKind.DataGovernance;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthControlKind));
        }
    }
}
