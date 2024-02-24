// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Adapter to convert Health report type between model and DTO.
/// </summary>
public static class TokenAdapter
{
    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static TokenModel ToModel(this TokenRequest dto)
    {
        return new TokenModel()
        {
            DatasetIds = dto.DatasetIds,
            ReportIds = dto.ReportIds,
        };
    }
}
