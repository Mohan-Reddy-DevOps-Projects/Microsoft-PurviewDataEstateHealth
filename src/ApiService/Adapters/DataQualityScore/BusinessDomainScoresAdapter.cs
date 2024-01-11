// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data quality scores model adapter for business domain
/// </summary>
[ModelAdapter(typeof(IDataQualityScoresModel), typeof(BusinessDomainDataQualityScore))]
public class BusinessDomainQualityScoreAdapter : BaseModelAdapter<IDataQualityScoresModel, BusinessDomainDataQualityScore>
{
    /// <inheritdoc />
    public override BusinessDomainDataQualityScore FromModel(IDataQualityScoresModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new BusinessDomainDataQualityScore
        {
            BusinessDomainId = model.BusinessDomainId,
            QualityScore = model.QualityScore,
            LastRefreshedAt = model.LastRefreshedAt
        };
    }

    /// <inheritdoc />
    public override IDataQualityScoresModel ToModel(BusinessDomainDataQualityScore dataQualityScoresDto)
    {
        return new DataQualityScoresModel
        {
            QualityScore = dataQualityScoresDto.QualityScore,
            LastRefreshedAt = dataQualityScoresDto.LastRefreshedAt,
            BusinessDomainId = dataQualityScoresDto.BusinessDomainId,
        };
    }
}
