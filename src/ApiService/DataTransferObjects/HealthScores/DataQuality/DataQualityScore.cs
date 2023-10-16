// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A data quality score class.
/// </summary>
public class DataQualityScore : HealthScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataQualityScore"/> class.
    /// </summary>
    public DataQualityScore()
    {
        this.ScoreKind = HealthScoreKind.DataQuality;
        this.Properties = new DataQualityScoreProperties();
    }

    /// <summary>
    /// Data quality score properties.
    /// </summary>
    [Required]
    public DataQualityScoreProperties Properties { get; set; }
}

