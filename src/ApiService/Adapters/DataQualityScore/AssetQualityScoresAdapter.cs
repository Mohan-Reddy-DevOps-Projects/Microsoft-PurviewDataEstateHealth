// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data quality scores model Adapter for asset
/// </summary>
[ModelAdapter(typeof(IDataQualityScoresModel), typeof(AssetDataQualityScore))]
public class AssetQualityScoresAdapter : BaseModelAdapter<IDataQualityScoresModel, AssetDataQualityScore>
{
    /// <inheritdoc />
    public override AssetDataQualityScore FromModel(IDataQualityScoresModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new AssetDataQualityScore
        {
            BusinessDomainId = model.BusinessDomainId,
            DataAssetId = model.DataAssetId,
            DataProductId = model.DataProductId,
            QualityScore = model.QualityScore,
            LastRefreshedAt = model.LastRefreshedAt
        };
    }

    /// <inheritdoc />
    public override IDataQualityScoresModel ToModel(AssetDataQualityScore dataQualityScoresDto)
    {
        return new DataQualityScoresModel
        {
            QualityScore = dataQualityScoresDto.QualityScore,
            LastRefreshedAt = dataQualityScoresDto.LastRefreshedAt,
            BusinessDomainId = dataQualityScoresDto.BusinessDomainId,
            DataProductId = dataQualityScoresDto.DataProductId,
            DataAssetId = dataQualityScoresDto.DataAssetId
        };
    }
}
