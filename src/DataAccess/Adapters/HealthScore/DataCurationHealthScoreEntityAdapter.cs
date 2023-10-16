// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataCurationHealthScoreEntity to HealthScoreModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(DataCurationScoreModel), typeof(DataCurationHealthScoreEntity))]
internal class DataCurationHealthScoreEntityAdapter : BaseModelAdapter<DataCurationScoreModel, DataCurationHealthScoreEntity>
{
    public override DataCurationHealthScoreEntity FromModel(DataCurationScoreModel model)
    {
        return new DataCurationHealthScoreEntity()
        {
            Name = model.Properties.Name,
            Description = model.Properties.Description,
            ScoreKind = model.Properties.ScoreKind,
            ReportId = model.Properties.ReportId,
            TotalCanBeCuratedCount = model.Properties.TotalCanBeCuratedCount,
            TotalCannotBeCuratedCount = model.Properties.TotalCannotBeCuratedCount,
            TotalCuratedCount = model.Properties.TotalCuratedCount,
        };
    }

    public override DataCurationScoreModel ToModel(DataCurationHealthScoreEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataCurationScoreModel()
        {
            Properties = new DataCurationScoreProperties()
            {
                ReportId = entity.ReportId,
                Name = entity.Name,
                Description = entity.Description,
                ScoreKind = entity.ScoreKind,
                TotalCuratedCount = entity.TotalCuratedCount,
                TotalCannotBeCuratedCount= entity.TotalCannotBeCuratedCount,
                TotalCanBeCuratedCount= entity.TotalCannotBeCuratedCount,
            }
        };
    }
}
