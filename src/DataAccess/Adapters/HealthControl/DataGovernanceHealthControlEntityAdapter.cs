// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Adapter for DataGovernanceHealthControlEntity to HealthControlModel conversions and vice versa.
/// </summary>
[ModelAdapter(typeof(DataGovernanceHealthControlModel), typeof(DataGovernanceHealthControlEntity))]
internal class DataGovernanceHealthControlEntityAdapter : BaseModelAdapter<DataGovernanceHealthControlModel, DataGovernanceHealthControlEntity>
{
    public override DataGovernanceHealthControlEntity FromModel(DataGovernanceHealthControlModel model)
    {
        return new DataGovernanceHealthControlEntity()
        {
            CurrentScore = model.Properties.CurrentScore,
            Kind = model.Properties.Kind,
            LastRefreshDate = model.Properties.LastRefreshedAt
        };
    }

    public override DataGovernanceHealthControlModel ToModel(DataGovernanceHealthControlEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new DataGovernanceHealthControlModel()
        {
            Properties = new DataGovernanceHealthControlProperties()
            {
                CurrentScore = entity.CurrentScore,
                Kind = entity.Kind,
                LastRefreshedAt = entity.LastRefreshDate
            }
        };
    }
}
