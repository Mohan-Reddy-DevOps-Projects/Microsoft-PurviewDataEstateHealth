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

    /// <summary>
    /// Business domain Id
    /// </summary>
    Guid BusinessDomainId { get; }

    /// <summary>
    /// Data Product Id
    /// </summary>
    Guid DataProductId { get; }

    /// <summary>
    /// Data asset Id
    /// </summary>
    Guid DataAssetId { get; }
}
