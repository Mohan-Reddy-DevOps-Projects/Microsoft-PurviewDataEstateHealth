// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert trend kind between model and DTO.
/// </summary>
public static class TrendKindAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.TrendKind ToDto(this TrendKind model)
    {
        switch (model)
        {
            case TrendKind.OpenActions:
                return DataTransferObjects.TrendKind.OpenActions;
            case TrendKind.BusinessDomainCount:
                return DataTransferObjects.TrendKind.BusinessDomainCount;
            case TrendKind.DataProductCount:
                return DataTransferObjects.TrendKind.DataProductCount;
            case TrendKind.DataAssetCount:
                return DataTransferObjects.TrendKind.DataAssetCount;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(TrendKind));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static TrendKind ToModel(this DataTransferObjects.TrendKind dto)
    {
        switch (dto)
        {
            case DataTransferObjects.TrendKind.OpenActions:
                return TrendKind.OpenActions;
            case DataTransferObjects.TrendKind.BusinessDomainCount:
                return TrendKind.BusinessDomainCount;
            case DataTransferObjects.TrendKind.DataProductCount:
                return TrendKind.DataProductCount;
            case DataTransferObjects.TrendKind.DataAssetCount:
                return TrendKind.DataAssetCount;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.TrendKind));
        }
    }
}
