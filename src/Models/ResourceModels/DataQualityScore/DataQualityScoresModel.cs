// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the data quality scores model
/// </summary>
public class DataQualityScoresModel : IDataQualityScoresModel
{
    /// <summary>
    /// Data quality score
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// Last refreshed at
    /// </summary>
    public DateTime LastRefreshedAt { get; set; }
}
