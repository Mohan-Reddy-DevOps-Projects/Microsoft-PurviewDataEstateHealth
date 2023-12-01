// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Trend kind enum.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TrendKind
{
    /// <summary>
    /// Open actions
    /// </summary>
    OpenActions = 1,

    /// <summary>
    /// Business domain count
    /// </summary>
    BusinessDomainCount,

    /// <summary>
    /// Data product count
    /// </summary>
    DataProductCount,

    /// <summary>
    /// Data asset count
    /// </summary>
    DataAssetCount,
}
