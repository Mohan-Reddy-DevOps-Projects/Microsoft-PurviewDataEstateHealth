// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Report Type.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthReportType
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

