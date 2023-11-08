// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A data health summary data transfer object.
/// </summary>
public class DataEstateHealthSummary
{
    /// <summary>
    /// Health action summary class.
    /// </summary>
    [ReadOnly(true)]
    public HealthActionsSummary HealthActionsSummary { get; internal set; }

    /// <summary>
    /// Business domain summary class.
    /// </summary>
    [ReadOnly(true)]
    public BusinessDomainsSummary BusinessDomainsSummary { get; internal set; }

    /// <summary>
    /// Data product summary class.
    /// </summary>
    [ReadOnly(true)]
    public DataProductsSummary DataProductsSummary { get; internal set; }

    /// <summary>
    /// Data asset summary class.
    /// </summary>
    [ReadOnly(true)]
    public DataAssetsSummary DataAssetsSummary { get; internal set; }
}
