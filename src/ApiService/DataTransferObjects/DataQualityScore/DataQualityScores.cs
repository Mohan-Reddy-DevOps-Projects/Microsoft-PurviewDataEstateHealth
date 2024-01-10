// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A quality scores class.
/// </summary>
public class DataQualityScores
{
    /// <summary>
    /// Quality score 
    /// </summary>
    [ReadOnly(true)]
    public double QualityScore { get; internal set; }

    /// <summary>
    /// Last refreshed at timestamp
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshedAt { get; internal set; }
}
