// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A data quality health control class.
/// </summary>
public class DataQualityHealthControl : HealthControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataQualityHealthControl"/> class.
    /// </summary>
    public DataQualityHealthControl()
    {
        this.Kind = HealthControlKind.DataQuality;
        this.Properties = new DataQualityHealthControlProperties();
    }

    /// <summary>
    /// Data Quality Health Control properties.
    /// </summary>
    [Required]
    public DataQualityHealthControlProperties Properties { get; set; }
}
