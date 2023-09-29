// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health report status between model and DTO.
/// </summary>
public static class HealthReportStatusAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthReportStatus ToDto(this HealthReportStatus model)
    {
        switch (model)
        {
            case HealthReportStatus.Draft:
                return DataTransferObjects.HealthReportStatus.Draft;
            case HealthReportStatus.Active:
                return DataTransferObjects.HealthReportStatus.Active;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthReportStatus));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthReportStatus ToModel(this DataTransferObjects.HealthReportStatus dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthReportStatus.Active:
                return HealthReportStatus.Active;
            case DataTransferObjects.HealthReportStatus.Draft:
                return HealthReportStatus.Draft;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthReportStatus));
        }
    }
}
