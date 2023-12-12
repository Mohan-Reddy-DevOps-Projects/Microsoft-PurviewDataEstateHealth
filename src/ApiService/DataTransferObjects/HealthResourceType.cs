// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health resource Type.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthResourceType
{
    /// <summary>
    /// System health report.
    /// </summary>
    System = 1,

    /// <summary>
    /// Custom health report.
    /// </summary>
    Custom
}
