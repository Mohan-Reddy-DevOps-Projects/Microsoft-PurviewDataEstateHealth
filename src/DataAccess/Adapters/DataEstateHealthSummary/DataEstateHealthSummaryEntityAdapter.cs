// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataEstateHealthSummaryEntity to DataEstateHealthSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IDataEstateHealthSummaryModel), typeof(DataEstateHealthSummaryEntity))]
internal class DataEstateHealthSummaryEntityAdapter : BaseModelAdapter<IDataEstateHealthSummaryModel, DataEstateHealthSummaryEntity>
{
    /// <inheritdoc />
    public override DataEstateHealthSummaryEntity FromModel(IDataEstateHealthSummaryModel model)
    {
        var businessDomainsSummaryAdapter = this.Builder.AdapterFor<IBusinessDomainsSummaryModel, BusinessDomainsSummaryEntity>();
        var dataProductsSummaryAdapter = this.Builder.AdapterFor<IDataProductsSummaryModel, DataProductsSummaryEntity>();
        var dataAssetsSummaryAdapter = this.Builder.AdapterFor<IDataAssetsSummaryModel, DataAssetsSummaryEntity>();
        var healthActionsSummaryAdapter = this.Builder.AdapterFor<IHealthActionsSummaryModel, HealthActionsSummaryEntity>();

        return new DataEstateHealthSummaryEntity
        {
            HealthActionsSummaryEntity = healthActionsSummaryAdapter.FromModel(model.HealthActionsSummaryModel),
            DataProductsSummaryEntity = dataProductsSummaryAdapter.FromModel(model.DataProductsSummaryModel),
            DataAssetsSummaryEntity = dataAssetsSummaryAdapter.FromModel(model.DataAssetsSummaryModel),
            BusinessDomainsSummaryEntity = businessDomainsSummaryAdapter.FromModel(model.BusinessDomainsSummaryModel)
        };
    }

    /// <inheritdoc />
    public override IDataEstateHealthSummaryModel ToModel(DataEstateHealthSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        var businessDomainsSummaryAdapter = this.Builder.AdapterFor<IBusinessDomainsSummaryModel, BusinessDomainsSummaryEntity>();
        var dataProductsSummaryAdapter = this.Builder.AdapterFor<IDataProductsSummaryModel, DataProductsSummaryEntity>();
        var dataAssetsSummaryAdapter = this.Builder.AdapterFor<IDataAssetsSummaryModel, DataAssetsSummaryEntity>();
        var healthActionsSummaryAdapter = this.Builder.AdapterFor<IHealthActionsSummaryModel, HealthActionsSummaryEntity>();

        return new DataEstateHealthSummaryModel
        {
            BusinessDomainsSummaryModel = businessDomainsSummaryAdapter.ToModel(entity.BusinessDomainsSummaryEntity),
            DataProductsSummaryModel = dataProductsSummaryAdapter.ToModel(entity.DataProductsSummaryEntity),
            DataAssetsSummaryModel = dataAssetsSummaryAdapter.ToModel(entity.DataAssetsSummaryEntity),
            HealthActionsSummaryModel = healthActionsSummaryAdapter.ToModel(entity.HealthActionsSummaryEntity)
        };
    }
}
