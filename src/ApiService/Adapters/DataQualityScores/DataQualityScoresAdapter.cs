// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data quality scores model Adapter
/// </summary>
[ModelAdapter(typeof(IDataQualityScoresModel), typeof(DataQualityScores))]
public class DataQualityScoresAdapter : BaseModelAdapter<IDataQualityScoresModel, DataQualityScores>
{
    /// <inheritdoc />
    public override DataQualityScores FromModel(IDataQualityScoresModel model)
    {
        if (model == null)
        {
            return null;
        }

        return new DataQualityScores
        {
            QualityScore = model.QualityScore,
            LastRefreshedAt = model.LastRefreshedAt
        };
    }

    /// <inheritdoc />
    public override IDataQualityScoresModel ToModel(DataQualityScores dataQualityScoresDto)
    {
        return new DataQualityScoresModel
        {
            QualityScore = dataQualityScoresDto.QualityScore,
            LastRefreshedAt = dataQualityScoresDto.LastRefreshedAt,
        };
    }
}
