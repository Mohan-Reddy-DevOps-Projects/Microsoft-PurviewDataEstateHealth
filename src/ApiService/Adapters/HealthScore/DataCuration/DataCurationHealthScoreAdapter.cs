// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles data curation score model and resource conversions.
/// </summary>
[ModelAdapter(typeof(DataCurationScoreModel), typeof(DataCurationScore))]
public class DataCurationHealthScoreAdapter : BaseModelAdapter<DataCurationScoreModel, DataCurationScore>
{
    /// <inheritdoc />
    public override DataCurationScoreModel ToModel(DataCurationScore resource)
    {
        return new DataCurationScoreModel
        {
            Properties = new Models.DataCurationScoreProperties
            {
                ScoreKind = resource.ScoreKind.ToModel(),
                Name = resource.Name,
                Description = resource.Description,
                ReportId = resource.ReportId,
                TotalCuratedCount = resource.Properties.TotalCuratedCount,
                TotalCanBeCuratedCount = resource.Properties.TotalCanBeCuratedCount,
                TotalCannotBeCuratedCount = resource.Properties.TotalCannotBeCuratedCount
            },
        };
    }

    /// <inheritdoc />
    public override DataCurationScore FromModel(DataCurationScoreModel model)
    {
        return new DataCurationScore
        {
            ScoreKind = model.Properties.ScoreKind.ToDto(),
            Name = model.Properties.Name,
            Description = model.Properties.Description,
            ReportId = model.Properties.ReportId,
            Properties = new DataCurationScoreProperties
            {
                TotalCuratedCount = model.Properties.TotalCuratedCount,
                TotalCanBeCuratedCount = model.Properties.TotalCanBeCuratedCount,
                TotalCannotBeCuratedCount = model.Properties.TotalCannotBeCuratedCount
            }
        };
    }
}
