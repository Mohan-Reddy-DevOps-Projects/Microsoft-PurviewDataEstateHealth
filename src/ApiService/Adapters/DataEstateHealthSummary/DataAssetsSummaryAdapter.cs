// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data Asset Summary Adapter
/// </summary>
[ModelAdapter(typeof(IDataAssetsSummaryModel), typeof(DataAssetsSummary))]
public class DataAssetsSummaryAdapter : BaseModelAdapter<IDataAssetsSummaryModel, DataAssetsSummary>
{
    /// <inheritdoc />
    public override DataAssetsSummary FromModel(IDataAssetsSummaryModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new DataAssetsSummary
        {
            TotalCuratedDataAssetsCount = model.TotalCuratedDataAssetsCount,
            TotalCuratableDataAssetsCount = model.TotalCuratableDataAssetsCount,
            TotalNonCuratableDataAssetsCount = model.TotalNonCuratableDataAssetsCount,
            LastRefreshDate = model.DataAssetsLastRefreshDate,
            DataAssetsTrendLink = model.DataAssetsTrendLink,
        };
    }

    /// <inheritdoc />
    public override IDataAssetsSummaryModel ToModel(DataAssetsSummary dataAssetSummaryDto)
    {
        return new DataAssetsSummaryModel
        {
            TotalCuratedDataAssetsCount = dataAssetSummaryDto.TotalCuratedDataAssetsCount,
            TotalCuratableDataAssetsCount = dataAssetSummaryDto.TotalCuratableDataAssetsCount,
            TotalNonCuratableDataAssetsCount = dataAssetSummaryDto.TotalNonCuratableDataAssetsCount,
            DataAssetsLastRefreshDate = dataAssetSummaryDto.LastRefreshDate,
            DataAssetsTrendLink = dataAssetSummaryDto.DataAssetsTrendLink
        };
    }
}
