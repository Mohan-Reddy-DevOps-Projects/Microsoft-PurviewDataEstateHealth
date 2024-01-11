// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data quality scores model adapter for product
/// </summary>
[ModelAdapter(typeof(IDataQualityScoresModel), typeof(ProductDataQualityScore))]
public class ProductQualityScoreAdapter : BaseModelAdapter<IDataQualityScoresModel, ProductDataQualityScore>
{
    /// <inheritdoc />
    public override ProductDataQualityScore FromModel(IDataQualityScoresModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new ProductDataQualityScore
        {
            BusinessDomainId = model.BusinessDomainId,
            DataProductId = model.DataProductId,
            QualityScore = model.QualityScore,
            LastRefreshedAt = model.LastRefreshedAt
        };
    }

    /// <inheritdoc />
    public override IDataQualityScoresModel ToModel(ProductDataQualityScore dataQualityScoresDto)
    {
        return new DataQualityScoresModel
        {
            QualityScore = dataQualityScoresDto.QualityScore,
            LastRefreshedAt = dataQualityScoresDto.LastRefreshedAt,
            BusinessDomainId = dataQualityScoresDto.BusinessDomainId,
            DataProductId = dataQualityScoresDto.DataProductId,
        };
    }
}
