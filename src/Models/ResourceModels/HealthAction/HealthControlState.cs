// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health control state enum.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthControlState
{
    /// <summary>
    /// Active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Resolved
    /// </summary>
    Resolved
}
