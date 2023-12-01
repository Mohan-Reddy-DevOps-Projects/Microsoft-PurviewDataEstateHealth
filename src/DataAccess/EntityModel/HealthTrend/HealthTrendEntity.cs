// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

internal class HealthTrendEntity : BaseEntity
{
    public HealthTrendEntity()
    {
    }

    public HealthTrendEntity(HealthTrendEntity entity)
    {
        this.Kind = entity.Kind;
        this.Description = entity.Description;
        this.Delta = entity.Delta;
        this.TrendValuesList = entity.TrendValuesList;
    }

    [JsonProperty("kind")]
    public TrendKind Kind { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("delta")]
    public int Delta { get; set; }

    [JsonProperty("trendValuesList")]
    public IEnumerable<TrendValue> TrendValuesList { get; set; }
}
