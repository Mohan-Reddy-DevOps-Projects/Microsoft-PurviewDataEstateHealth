// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data Products Summary Adapter
/// </summary>
[ModelAdapter(typeof(IDataProductsSummaryModel), typeof(DataProductsSummary))]
public class DataProductsSummaryAdapter : BaseModelAdapter<IDataProductsSummaryModel, DataProductsSummary>
{
    /// <inheritdoc />
    public override DataProductsSummary FromModel(IDataProductsSummaryModel model)
    {
        return new DataProductsSummary
        {
            TotalDataProductsCount = model.TotalDataProductsCount,
            DataProductsTrendLink = model.DataProductsTrendLink,
            LastRefreshDate = model.DataProductsLastRefreshDate
        };
    }

    /// <inheritdoc />
    public override IDataProductsSummaryModel ToModel(DataProductsSummary dataProductSummaryDto)
    {
        return new DataProductsSummaryModel
        {
            TotalDataProductsCount = dataProductSummaryDto.TotalDataProductsCount,
            DataProductsTrendLink = dataProductSummaryDto.DataProductsTrendLink,
            DataProductsLastRefreshDate = dataProductSummaryDto.LastRefreshDate
        };
    }
}
