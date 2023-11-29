// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;

[HealthControlEntity(HealthControlKind.DataQuality)]
internal class DataQualityHealthControlEntity : HealthControlEntity
{
    public DataQualityHealthControlEntity()
    {
    }

    public DataQualityHealthControlEntity(DataQualityHealthControlEntity entity)
    {
        this.CurrentScore = entity.CurrentScore;
        this.LastRefreshedAt = entity.LastRefreshedAt;
    }
}
