// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

[JsonConverter(typeof(HealthScoreEntityConverter))]
internal class HealthScoreEntity : BaseEntity
{
    public HealthScoreEntity()
    {
    }

    public HealthScoreEntity(HealthScoreEntity entity)
    {
        this.ScoreKind = entity.ScoreKind;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.ReportId = entity.ReportId;
    }

    [JsonProperty("scoreKind")]
    public HealthScoreKind ScoreKind { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("reportId")]
    public Guid ReportId { get; set; }
}
