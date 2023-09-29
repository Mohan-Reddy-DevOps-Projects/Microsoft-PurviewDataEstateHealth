// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health report type between model and DTO.
/// </summary>
public static class HealthReportTypeAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthReportType ToDto(this HealthReportType model)
    {
        switch (model)
        {
            case HealthReportType.System:
                return DataTransferObjects.HealthReportType.System;
            case HealthReportType.Custom:
                return DataTransferObjects.HealthReportType.Custom;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthReportType));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthReportType ToModel(this DataTransferObjects.HealthReportType dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthReportType.System:
                return HealthReportType.System;
            case DataTransferObjects.HealthReportType.Custom:
                return HealthReportType.Custom;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthReportType));
        }
    }
}
