// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataQualityHealthControlEntity to HealthControlModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(DataQualityHealthControlModel), typeof(DataQualityHealthControlEntity))]
internal class DataQualityHealthControlEntityAdapter : BaseModelAdapter<DataQualityHealthControlModel, DataQualityHealthControlEntity>
{
    public override DataQualityHealthControlEntity FromModel(DataQualityHealthControlModel model)
    {
        return new DataQualityHealthControlEntity()
        {
            CurrentScore = model.Properties.CurrentScore,
            Kind = model.Properties.Kind,
            LastRefreshedAt = model.Properties.LastRefreshedAt
        };
    }

    public override DataQualityHealthControlModel ToModel(DataQualityHealthControlEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataQualityHealthControlModel()
        {
            Properties = new DataQualityHealthControlProperties()
            {
                CurrentScore = entity.CurrentScore,
                Kind = entity.Kind,
                LastRefreshedAt = entity.LastRefreshedAt
            }
        };
    }
}
