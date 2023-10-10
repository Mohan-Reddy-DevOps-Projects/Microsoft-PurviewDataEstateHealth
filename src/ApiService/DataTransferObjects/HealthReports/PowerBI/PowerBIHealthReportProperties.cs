// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// PowerBI health report property bag
/// </summary>
public class PowerBIHealthReportProperties
{
    /// <summary>
    /// Report description.
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; set; }

    /// <summary>
    /// Embed Link
    /// </summary>
    [ReadOnly(true)]
    public string EmbedLink {  get; internal set; } 
}
