// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataAssetsSummaryEntity to DataAssetsSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IDataAssetsSummaryModel), typeof(DataAssetsSummaryEntity))]
internal class DataAssetsSummaryEntityAdapter : BaseModelAdapter<IDataAssetsSummaryModel, DataAssetsSummaryEntity>
{
    public override DataAssetsSummaryEntity FromModel(IDataAssetsSummaryModel model)
    {
        return new DataAssetsSummaryEntity()
        {
            TotalCuratableDataAssetsCount = model.TotalCuratableDataAssetsCount,
            TotalNonCuratableDataAssetsCount = model.TotalNonCuratableDataAssetsCount,
            TotalCuratedDataAssetsCount = model.TotalCuratedDataAssetsCount,
            DataAssetsLastRefreshDate = model.DataAssetsLastRefreshDate,
            DataAssetsTrendLink = model.DataAssetsTrendLink
        };
    }

    public override IDataAssetsSummaryModel ToModel(DataAssetsSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataAssetsSummaryModel()
        {
            TotalCuratableDataAssetsCount = entity.TotalCuratableDataAssetsCount,
            TotalNonCuratableDataAssetsCount = entity.TotalNonCuratableDataAssetsCount,
            TotalCuratedDataAssetsCount = entity.TotalCuratedDataAssetsCount,
            DataAssetsLastRefreshDate = entity.DataAssetsLastRefreshDate,
            DataAssetsTrendLink = entity.DataAssetsTrendLink
        };
    }
}
