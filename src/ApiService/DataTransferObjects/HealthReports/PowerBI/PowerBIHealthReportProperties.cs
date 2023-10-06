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
    /// Created At
    /// </summary>
    [ReadOnly(true)]
    public string CreatedAt { get; internal set; }

    /// <summary>
    /// Created By
    /// </summary>
    [ReadOnly(true)]
    public string CreatedBy { get; internal set; }

    /// <summary>
    /// Modified At
    /// </summary>
    [ReadOnly(true)]
    public string ModifiedAt { get; internal set; }

    /// <summary>
    /// Modiifed By
    /// </summary>
    [ReadOnly(true)]
    public string ModifiedBy {  get; internal set; }

    /// <summary>
    /// Embed Link
    /// </summary>
    [ReadOnly(true)]
    public string EmbedLink {  get; internal set; } 
}
