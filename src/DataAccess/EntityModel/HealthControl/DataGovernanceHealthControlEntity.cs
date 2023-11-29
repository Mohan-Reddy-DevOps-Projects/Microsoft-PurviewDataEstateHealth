// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;

[HealthControlEntity(HealthControlKind.DataGovernance)]
internal class DataGovernanceHealthControlEntity : HealthControlEntity
{
    public DataGovernanceHealthControlEntity()
    {
    }

    public DataGovernanceHealthControlEntity(DataGovernanceHealthControlEntity entity)
    {
        this.CurrentScore = entity.CurrentScore;
        this.LastRefreshDate = entity.LastRefreshDate;
    }
}
