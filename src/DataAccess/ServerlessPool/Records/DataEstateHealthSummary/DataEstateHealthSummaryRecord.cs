// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for data estate health summary.
/// </summary>
public class DataEstateHealthSummaryRecord 
{
    /// <summary>
    /// Business domain Id.
    /// </summary>
    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Total business domains.
    /// </summary>
    [DataColumn("TotalBusinessDomains")]
    public int TotalBusinessDomains { get; set; }

    /// <summary>
    /// Last refresh date.
    /// </summary>
    [DataColumn("LastRefreshDate")]
    public DateTime LastRefreshDate { get; set; }

    /// <summary>
    /// Business Domains Filter List Link.
    /// </summary>
    [DataColumn("BusinessDomainsFilterListLink")]
    public string BusinessDomainsFilterListLink { get; set; }

    /// <summary>
    /// Business Domains Trend Link.
    /// </summary>
    [DataColumn("BusinessDomainsTrendLink")]
    public string BusinessDomainsTrendLink { get; set; }

    /// <summary>
    /// Total Curated Data Assets Count.
    /// </summary>
    [DataColumn("TotalCuratedDataAssetsCount")]
    public int TotalCuratedDataAssetsCount { get; set; }

    /// <summary>
    /// Total Curatable Data Assets Count.
    /// </summary>
    [DataColumn("TotalCuratableDataAssetsCount")]
    public int TotalCuratableDataAssetsCount { get; set; }

    /// <summary>
    /// Total Non Curatable Data Assets Count.
    /// </summary>
    [DataColumn("TotalNonCuratableDataAssetsCount")]
    public int TotalNonCuratableDataAssetsCount { get; set; }

    /// <summary>
    /// Data Assets Trend Link.
    /// </summary>
    [DataColumn("DataAssetsTrendLink")]
    public string DataAssetsTrendLink { get; set; }

    /// <summary>
    /// Total data products.
    /// </summary>
    [DataColumn("TotalDataProductsCount")]
    public int TotalDataProductsCount { get; set; }

    /// <summary>
    /// Data Products Trend Link.
    /// </summary>
    [DataColumn("DataProductsTrendLink")]
    public string DataProductsTrendLink { get; set; }

    /// <summary>
    /// Total Open Actions Count.
    /// </summary>
    [DataColumn("TotalOpenActionsCount")]
    public int TotalOpenActionsCount { get; set; }

    /// <summary>
    /// Total Completed Actions Count.
    /// </summary>
    [DataColumn("TotalCompletedActionsCount")]
    public int TotalCompletedActionsCount { get; set; }

    /// <summary>
    /// Total Dismissed Actions Count.
    /// </summary>
    [DataColumn("TotalDismissedActionsCount")]
    public int TotalDismissedActionsCount { get; set; }

    /// <summary>
    /// Health Actions Trend Link.
    /// </summary>
    [DataColumn("HealthActionsTrendLink")]
    public string HealthActionsTrendLink { get; set; }
}
