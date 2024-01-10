// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

/// <summary>
/// Data Quality Score List Context
/// </summary>
public interface IDataQualityContext : IDataQualityListContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Data product id
    /// </summary>
    Guid DataProductId { get; set; }

    /// <summary>
    /// Data asset id
    /// </summary>
    Guid DataAssetId { get; set; }
}
