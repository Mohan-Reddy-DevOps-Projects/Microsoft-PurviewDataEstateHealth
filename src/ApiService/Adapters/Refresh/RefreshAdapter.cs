// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;
/// <summary>
/// Adapter to convert refresh between model and DTO.
/// </summary>
public static class RefreshAdapter
{
    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static Refresh FromModel(this PowerBI.Api.Models.Refresh dto)
    {
        return new Refresh()
        {
            Id = dto.RequestId,
            Status = dto.Status,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
        };
    }
}
