// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Report status.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthReportStatus
{
    /// <summary>
    /// Active health report
    /// </summary>
    Active = 1,

    /// <summary>
    /// Draft report.
    /// </summary>
    Draft
}
