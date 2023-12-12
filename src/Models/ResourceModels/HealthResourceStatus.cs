// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.ComponentModel;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Report status.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthResourceStatus
{
    /// <summary>
    /// Active health report
    /// </summary>
    [Description("Active")]
    Active = 1,

    /// <summary>
    /// Draft report.
    /// </summary>
    [Description("Draft")]
    Draft,

    /// <summary>
    /// Not started
    /// </summary>
    [Description("Not started")]
    NotStarted
}
