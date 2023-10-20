// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health control kind.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthControlKind
{
    /// <summary>
    /// DataGovernance Control.
    /// </summary>
    DataGovernance = 1,

    /// <summary>
    /// Data quality control.
    /// </summary>
    DataQuality,  
}
