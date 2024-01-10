// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the data quality scores model
/// </summary>
public interface IDataQualityScoresModel
{
    /// <summary>
    /// Data quality score
    /// </summary>
    double QualityScore { get; }

    /// <summary>
    /// Last refreshed at
    /// </summary>
    DateTime LastRefreshedAt { get; }
}
