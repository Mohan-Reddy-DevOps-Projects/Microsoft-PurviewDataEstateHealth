// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
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
    /// Activity
    /// </summary>
    Activity,
}
