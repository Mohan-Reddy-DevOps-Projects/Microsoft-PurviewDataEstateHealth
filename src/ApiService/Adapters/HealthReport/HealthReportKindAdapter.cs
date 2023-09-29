// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert Health report kind between model and DTO.
/// </summary>
public static class HealthReportKindAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.HealthReportKind ToDto(this HealthReportKind model)
    {
        switch (model)
        {
            case HealthReportKind.LegacyHealthReport:
                return DataTransferObjects.HealthReportKind.LegacyHealthReport;
            case HealthReportKind.PowerBIHealthReport:
                return DataTransferObjects.HealthReportKind.PowerBIHealthReport;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(HealthReportKind));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static HealthReportKind ToModel(this DataTransferObjects.HealthReportKind dto)
    {
        switch (dto)
        {
            case DataTransferObjects.HealthReportKind.LegacyHealthReport:
                return HealthReportKind.LegacyHealthReport;
            case DataTransferObjects.HealthReportKind.PowerBIHealthReport:
                return HealthReportKind.PowerBIHealthReport;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.HealthReportKind));
        }
    }
}
