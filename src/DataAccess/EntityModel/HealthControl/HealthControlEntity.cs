// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

[JsonConverter(typeof(HealthControlEntityConverter))]
internal class HealthControlEntity : BaseEntity
{
    public HealthControlEntity()
    {
    }

    public HealthControlEntity(HealthControlEntity entity)
    {
        this.CurrentScore = entity.CurrentScore;
        this.LastRefreshedAt = entity.LastRefreshedAt;
        this.Kind = entity.Kind;
    }

    [JsonProperty("kind")]
    public HealthControlKind Kind { get; set; }

    [JsonProperty("currentScore")]
    public double CurrentScore { get; set; }

    [JsonProperty("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
