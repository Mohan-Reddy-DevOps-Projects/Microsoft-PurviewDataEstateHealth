// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// Data curation score property bag
/// </summary>
public class DataCurationScoreProperties 
{
    /// <summary>
    /// Total curated count.
    /// </summary>
    [ReadOnly(true)]
    public string TotalCuratedCount { get; internal set; }

    /// <summary>
    /// Total can be curated count.
    /// </summary>
    [ReadOnly(true)]
    public string TotalCanBeCuratedCount { get; internal set; }

    /// <summary>
    /// Total cannot be curated count.
    /// </summary>
    [ReadOnly(true)]
    public string TotalCannotBeCuratedCount { get; internal set; }
}
