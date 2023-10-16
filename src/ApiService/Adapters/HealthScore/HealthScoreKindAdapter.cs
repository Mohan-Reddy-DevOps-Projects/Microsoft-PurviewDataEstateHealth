// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health score kind between model and DTO.
/// </summary>
public static class HealthScoreKindAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthScoreKind ToDto(this HealthScoreKind model)
    {
        switch (model)
        {
            case HealthScoreKind.DataGovernance:
                return DataTransferObjects.HealthScoreKind.DataGovernance;
            case HealthScoreKind.DataCuration:
                return DataTransferObjects.HealthScoreKind.DataCuration;
            case HealthScoreKind.DataQuality:
                return DataTransferObjects.HealthScoreKind.DataQuality;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthScoreKind));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthScoreKind ToModel(this DataTransferObjects.HealthScoreKind dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthScoreKind.DataGovernance:
                return HealthScoreKind.DataGovernance;
            case DataTransferObjects.HealthScoreKind.DataCuration:
                return HealthScoreKind.DataCuration;
            case DataTransferObjects.HealthScoreKind.DataQuality:
                return HealthScoreKind.DataQuality;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthScoreKind));
        }
    }
}
