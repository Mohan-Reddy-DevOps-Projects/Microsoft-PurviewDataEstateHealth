// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// PowerBI health report property bag
/// </summary>
public class PowerBIHealthReportProperties : HealthReportProperties
{
    /// <summary>
    /// Dataset Id
    /// </summary>
    [ReadOnly(true)]
    public Guid DatasetId { get; internal set; }

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
