// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// Legacy health report property bag
/// </summary>
public class LegacyHealthReportProperties : HealthReportProperties
{
    /// <summary>
    /// Embed Link
    /// </summary>
    [ReadOnly(true)]
    public string EmbedLink { get; internal set; }

    /// <summary>
    /// Report description.
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; set; }
}
