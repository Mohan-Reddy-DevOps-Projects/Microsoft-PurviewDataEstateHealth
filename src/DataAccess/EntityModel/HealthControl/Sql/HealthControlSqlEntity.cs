// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

internal class HealthControlSqlEntity : BaseEntity, IHealthControlSqlEntity 
{
    public HealthControlSqlEntity()
    {
    }

    public HealthControlSqlEntity(HealthControlSqlEntity entity)
    {
        this.CurrentScore = entity.CurrentScore;
        this.LastRefreshedAt = entity.LastRefreshedAt;
    }

    [JsonProperty("currentScore")]
    public double CurrentScore { get; set; }

    [JsonProperty("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
