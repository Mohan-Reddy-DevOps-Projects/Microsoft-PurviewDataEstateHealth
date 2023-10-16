// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A data governance score class.
/// </summary>
public class DataGovernanceScore : HealthScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGovernanceScore"/> class.
    /// </summary>
    public DataGovernanceScore()
    {
        this.ScoreKind = HealthScoreKind.DataGovernance;
        this.Properties = new DataGovernanceScoreProperties();
    }

    /// <summary>
    /// Data health score properties.
    /// </summary>
    [Required]
    public DataGovernanceScoreProperties Properties { get; set; }
}

