// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A data curation score class.
/// </summary>
public class DataCurationScore : HealthScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCurationScore"/> class.
    /// </summary>
    public DataCurationScore()
    {
        this.ScoreKind = HealthScoreKind.DataCuration;
        this.Properties = new DataCurationScoreProperties();
    }

    /// <summary>
    /// Data Curation score properties.
    /// </summary>
    [Required]
    public DataCurationScoreProperties Properties { get; set; }
}

