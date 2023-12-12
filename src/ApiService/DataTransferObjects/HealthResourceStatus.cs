// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health resource status.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthResourceStatus
{
    /// <summary>
    /// Active health report
    /// </summary>
    Active = 1,

    /// <summary>
    /// Draft report.
    /// </summary>
    Draft,

    /// <summary>
    /// Not started
    /// </summary>
    NotStarted
}
