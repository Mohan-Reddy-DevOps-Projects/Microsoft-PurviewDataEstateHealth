// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

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
        this.OwnershipScore = entity.OwnershipScore;
        this.QualityScore = entity.QualityScore;
        this.DataQualityScore = entity.DataQualityScore;
        this.CatalogingScore = entity.CatalogingScore;
        this.AccessEntitlementScore = entity.AccessEntitlementScore;
        this.AuthoritativeDataSourceScore = entity.AuthoritativeDataSourceScore;
        this.ClassificationScore = entity.ClassificationScore;
        this.DataConsumptionPurposeScore = entity.DataConsumptionPurposeScore;
        this.UseScore = entity.UseScore;
        this.MetadataCompletenessScore = entity.MetadataCompletenessScore;
    }

    [JsonProperty("currentScore")]
    public double CurrentScore { get; set; }

    [JsonProperty("currentScore")]
    public double OwnershipScore { get; set; }

    [JsonProperty("metadataCompletenessScore")]
    public double MetadataCompletenessScore { get; set; }

    [JsonProperty("catalogingScore")]
    public double CatalogingScore { get; set; }

    [JsonProperty("classificationScore")]
    public double ClassificationScore { get; set; }

    [JsonProperty("useScore")]
    public double UseScore { get; set; }

    [JsonProperty("dataConsumptionPurposeScore")]
    public double DataConsumptionPurposeScore { get; set; }

    [JsonProperty("accessEntitlementScore")]
    public double AccessEntitlementScore { get; set; }

    [JsonProperty("qualityScore")]
    public double QualityScore { get; set; }

    [JsonProperty("dataQualityScore")]
    public double DataQualityScore { get; set; }

    [JsonProperty("authoritativeDataSourceScore")]
    public double AuthoritativeDataSourceScore { get; set; }

    [JsonProperty("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
