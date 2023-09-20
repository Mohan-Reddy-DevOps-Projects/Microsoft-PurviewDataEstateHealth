// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data Estate Health Summary Adapter
/// </summary>
[ModelAdapter(typeof(IDataEstateHealthSummaryModel), typeof(DataEstateHealthSummary))]
public class DataEstateHealthSummarydapter : BaseModelAdapter<IDataEstateHealthSummaryModel, DataEstateHealthSummary>
{
    /// <inheritdoc />
    public override DataEstateHealthSummary FromModel(IDataEstateHealthSummaryModel model)
    {
        var businessDomainsSummaryAdapter = this.Builder.AdapterFor<IBusinessDomainsSummaryModel, BusinessDomainsSummary>();
        var dataProductsSummaryAdapter = this.Builder.AdapterFor<IDataProductsSummaryModel, DataProductsSummary>();
        var dataAssetsSummaryAdapter = this.Builder.AdapterFor<IDataAssetsSummaryModel, DataAssetsSummary>();
        var healthReportsSummaryAdapter = this.Builder.AdapterFor<IHealthReportsSummaryModel, HealthReportsSummary>();
        var healthActionsSummaryAdapter = this.Builder.AdapterFor<IHealthActionsSummaryModel, HealthActionsSummary>();

        return new DataEstateHealthSummary
        {
            HealthActionsSummary  = healthActionsSummaryAdapter.FromModel(model.HealthActionsSummaryModel),
            DataProductsSummary = dataProductsSummaryAdapter.FromModel(model.DataProductsSummaryModel),
            DataAssetsSummary = dataAssetsSummaryAdapter.FromModel(model.DataAssetsSummaryModel),
            BusinessDomainsSummary = businessDomainsSummaryAdapter.FromModel(model.BusinessDomainsSummaryModel),
            HealthReportsSummary = healthReportsSummaryAdapter.FromModel(model.HealthReportsSummaryModel)
        };
    }

    /// <inheritdoc />
    public override IDataEstateHealthSummaryModel ToModel(DataEstateHealthSummary dataEstateHealthSummaryDto)
    {
        var businessDomainsSummaryAdapter = this.Builder.AdapterFor<IBusinessDomainsSummaryModel, BusinessDomainsSummary>();
        var dataProductsSummaryAdapter = this.Builder.AdapterFor<IDataProductsSummaryModel, DataProductsSummary>();
        var dataAssetsSummaryAdapter = this.Builder.AdapterFor<IDataAssetsSummaryModel, DataAssetsSummary>();
        var healthReportsSummaryAdapter = this.Builder.AdapterFor<IHealthReportsSummaryModel, HealthReportsSummary>();
        var healthActionsSummaryAdapter = this.Builder.AdapterFor<IHealthActionsSummaryModel, HealthActionsSummary>();

        return new DataEstateHealthSummaryModel
        {
            BusinessDomainsSummaryModel = businessDomainsSummaryAdapter.ToModel(dataEstateHealthSummaryDto.BusinessDomainsSummary),
            DataProductsSummaryModel = dataProductsSummaryAdapter.ToModel(dataEstateHealthSummaryDto.DataProductsSummary),
            DataAssetsSummaryModel = dataAssetsSummaryAdapter.ToModel(dataEstateHealthSummaryDto.DataAssetsSummary),
            HealthReportsSummaryModel = healthReportsSummaryAdapter.ToModel(dataEstateHealthSummaryDto.HealthReportsSummary),
            HealthActionsSummaryModel = healthActionsSummaryAdapter.ToModel(dataEstateHealthSummaryDto.HealthActionsSummary)
        };
    }
}

