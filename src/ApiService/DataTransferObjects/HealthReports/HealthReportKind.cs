// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Report kind.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum HealthReportKind
{
    /// <summary>
    /// PowerBI health report
    /// </summary>
    PowerBIHealthReport = 1,

    /// <summary>
    /// Legacy report.
    /// </summary>
    LegacyHealthReport
}
