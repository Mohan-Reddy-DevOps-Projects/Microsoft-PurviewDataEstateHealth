// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

[HealthScoreEntity(HealthScoreKind.DataQuality)]
internal class DataQualityHealthScoreEntity : HealthScoreEntity
{
    public DataQualityHealthScoreEntity()
    {
    }

    public DataQualityHealthScoreEntity(DataQualityHealthScoreEntity entity)
    {
        this.ActualValue = entity.ActualValue;
        this.TargetValue = entity.TargetValue;
        this.PerformanceIndicatorRules = entity.PerformanceIndicatorRules;
        this.MeasureUnit = entity.MeasureUnit;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.ReportId = entity.ReportId;
        this.ScoreKind = entity.ScoreKind;
    }

    [JsonProperty("actualValue")]
    public int ActualValue { get; set; }

    [JsonProperty("targetValue")]
    public int TargetValue { get; set; }

    [JsonProperty("measureUnit")]
    public string MeasureUnit { get; set; }

    [JsonProperty("performanceIndicatorRules")]
    public IEnumerable<PerformanceIndicatorRules> PerformanceIndicatorRules { get; set; }
}
