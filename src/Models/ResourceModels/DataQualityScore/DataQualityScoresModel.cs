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

    /// <summary>
    /// Business domain Id
    /// </summary>
    public Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Data Product Id
    /// </summary>
    public Guid DataProductId { get; set; }

    /// <summary>
    /// Data asset Id
    /// </summary>
    public Guid DataAssetId { get; set; }
}
