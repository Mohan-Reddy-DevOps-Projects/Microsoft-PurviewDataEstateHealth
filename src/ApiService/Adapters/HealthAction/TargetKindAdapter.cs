// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Adapter to convert target kind between model and DTO.
/// </summary>
public static class TargetKindAdapter
{
    /// <summary>
    /// Convert to DTO.
    /// </summary>
    /// <param name="model">Input model.</param>
    /// <returns></returns>
    public static DataTransferObjects.TargetKind ToDto(this TargetKind model)
    {
        switch (model)
        {
            case TargetKind.BusinessDomain:
                return DataTransferObjects.TargetKind.BusinessDomain;
            case TargetKind.DataProduct:
                return DataTransferObjects.TargetKind.DataProduct;
            case TargetKind.DataAsset:
                return DataTransferObjects.TargetKind.DataAsset;
            default:
                throw new InvalidEnumArgumentException(nameof(model), (int)model, typeof(TargetKind));
        }
    }

    /// <summary>
    /// Convert to model.
    /// </summary>
    /// <param name="dto">Input DTO.</param>
    /// <returns></returns>
    public static TargetKind ToModel(this DataTransferObjects.TargetKind dto)
    {
        switch (dto)
        {
            case DataTransferObjects.TargetKind.BusinessDomain:
                return TargetKind.BusinessDomain;
            case DataTransferObjects.TargetKind.DataProduct:
                return TargetKind.DataProduct;
            case DataTransferObjects.TargetKind.DataAsset:
                return TargetKind.DataAsset;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(dto),
                    (int)dto,
                    typeof(DataTransferObjects.TargetKind));
        }
    }
}
