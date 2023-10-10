// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Target kind enum.
/// </summary>
public enum TargetKind
{
    /// <summary>
    /// Business domain
    /// </summary>
    BusinessDomain = 1,

    /// <summary>
    /// Data product.
    /// </summary>
    DataProduct,

    /// <summary>
    /// Data asset.
    /// </summary>
    DataAsset
}
