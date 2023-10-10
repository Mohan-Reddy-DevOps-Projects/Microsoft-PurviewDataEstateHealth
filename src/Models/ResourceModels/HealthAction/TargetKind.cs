// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Target kind enum.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TargetKind
{
    /// <summary>
    /// Business domain.
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
