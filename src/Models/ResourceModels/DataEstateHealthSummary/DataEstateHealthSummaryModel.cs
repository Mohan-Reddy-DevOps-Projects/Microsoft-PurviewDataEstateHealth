// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc/> 
public class DataEstateHealthSummaryModel : IDataEstateHealthSummaryModel
{
    /// <inheritdoc />
    public IBusinessDomainsSummaryModel BusinessDomainsSummaryModel { get; set; }

    /// <inheritdoc />
    public IDataAssetsSummaryModel DataAssetsSummaryModel { get; set; }

    /// <inheritdoc />
    public IDataProductsSummaryModel DataProductsSummaryModel { get; set; }

    /// <inheritdoc />
    public IHealthActionsSummaryModel HealthActionsSummaryModel { get; set; }

    /// <inheritdoc />
    public IHealthReportsSummaryModel HealthReportsSummaryModel { get; set; }
}

