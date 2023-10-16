// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataGovernanceHealthScoreEntity to HealthScoreModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(DataGovernanceScoreModel), typeof(DataGovernanceHealthScoreEntity))]
internal class DataGovernanceHealthScoreEntityAdapter : BaseModelAdapter<DataGovernanceScoreModel, DataGovernanceHealthScoreEntity>
{
    public override DataGovernanceHealthScoreEntity FromModel(DataGovernanceScoreModel model)
    {
        return new DataGovernanceHealthScoreEntity()
        {
            Name = model.Properties.Name,
            Description = model.Properties.Description,
            ScoreKind = model.Properties.ScoreKind,
            ReportId = model.Properties.ReportId,
            TargetValue = model.Properties.TargetValue,
            ActualValue = model.Properties.ActualValue,
            MeasureUnit = model.Properties.MeasureUnit,
            PerformanceIndicatorRules = model.Properties.PerformanceIndicatorRules
        };
    }

    public override DataGovernanceScoreModel ToModel(DataGovernanceHealthScoreEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataGovernanceScoreModel()
        {
            Properties = new GovernanceAndQualityScoreProperties()
            {
                ReportId = entity.ReportId,
                Name = entity.Name,
                Description = entity.Description,
                ScoreKind = entity.ScoreKind,
                ActualValue = entity.ActualValue,
                PerformanceIndicatorRules = entity.PerformanceIndicatorRules,
                TargetValue = entity.TargetValue,
                MeasureUnit = entity.MeasureUnit,
            }
        };
    }
}
