// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Adapters;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataProductsSummaryEntity to DataProductsSummaryModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(IDataProductsSummaryModel), typeof(DataProductsSummaryEntity))]
internal class DataProductsSummaryEntityAdapter : BaseModelAdapter<IDataProductsSummaryModel, DataProductsSummaryEntity>
{
    public override DataProductsSummaryEntity FromModel(IDataProductsSummaryModel model)
    {
        return new DataProductsSummaryEntity()
        {
            DataProductsTrendLink = model.DataProductsTrendLink,
            DataProductsLastRefreshDate = model.DataProductsLastRefreshDate,
            TotalDataProductsCount  = model.TotalDataProductsCount,
        };
    }

    public override IDataProductsSummaryModel ToModel(DataProductsSummaryEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataProductsSummaryModel()
        {
            DataProductsTrendLink = entity.DataProductsTrendLink,
            DataProductsLastRefreshDate = entity.DataProductsLastRefreshDate,
            TotalDataProductsCount = entity.TotalDataProductsCount,
        };
    }
}
