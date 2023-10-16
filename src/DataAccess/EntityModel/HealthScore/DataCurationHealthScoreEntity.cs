// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

[HealthScoreEntity(Models.HealthScoreKind.DataCuration)]
internal class DataCurationHealthScoreEntity : HealthScoreEntity
{
    public DataCurationHealthScoreEntity()
    {
    }

    public DataCurationHealthScoreEntity(DataCurationHealthScoreEntity entity)
    {
        this.TotalCuratedCount = entity.TotalCuratedCount;
        this.TotalCanBeCuratedCount = entity.TotalCanBeCuratedCount;
        this.TotalCannotBeCuratedCount = entity.TotalCannotBeCuratedCount;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.ReportId = entity.ReportId;
        this.ScoreKind = entity.ScoreKind;
    }

    [JsonProperty("totalCuratedCount")]
    public string TotalCuratedCount { get; set; }

    [JsonProperty("totalCanBeCuratedCount")]
    public string TotalCanBeCuratedCount { get; set; }

    [JsonProperty("totalCannotBeCuratedCount")]
    public string TotalCannotBeCuratedCount { get; set; }
}
