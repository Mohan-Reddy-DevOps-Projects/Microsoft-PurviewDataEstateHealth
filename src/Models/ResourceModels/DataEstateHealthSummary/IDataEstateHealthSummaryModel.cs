// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the data estate health summary model.
/// </summary>
public interface IDataEstateHealthSummaryModel
{
    /// <summary>
    /// Business domain summary model
    /// </summary>
    IBusinessDomainsSummaryModel BusinessDomainsSummaryModel { get; }

    /// <summary>
    /// Data assets summary model
    /// </summary>
    IDataAssetsSummaryModel DataAssetsSummaryModel { get; }

    /// <summary>
    /// Data products summary model
    /// </summary>
    IDataProductsSummaryModel DataProductsSummaryModel { get; }

    /// <summary>
    /// Health actions summary model
    /// </summary>
    IHealthActionsSummaryModel HealthActionsSummaryModel { get; }

    /// <summary>
    /// Health reports summary model
    /// </summary>
    IHealthReportsSummaryModel HealthReportsSummaryModel { get; }
}
