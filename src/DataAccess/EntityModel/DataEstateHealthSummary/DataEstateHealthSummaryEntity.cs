// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class DataEstateHealthSummaryEntity
{
    public DataEstateHealthSummaryEntity()
    {

    }

    public DataEstateHealthSummaryEntity(DataEstateHealthSummaryEntity entity)
    {
        this.BusinessDomainsSummaryEntity = entity.BusinessDomainsSummaryEntity;
        this.DataProductsSummaryEntity = entity.DataProductsSummaryEntity;
        this.DataAssetsSummaryEntity = entity.DataAssetsSummaryEntity;
        this.HealthReportsSummaryEntity = entity.HealthReportsSummaryEntity;
        this.HealthActionsSummaryEntity = entity.HealthActionsSummaryEntity;  
    }

    [JsonProperty("businessDomainsSummary")]
    public BusinessDomainsSummaryEntity BusinessDomainsSummaryEntity { get; set; }

    [JsonProperty("dataAssetsSummary")]
    public DataAssetsSummaryEntity DataAssetsSummaryEntity { get; set; }

    [JsonProperty("dataProductsSummary")]
    public DataProductsSummaryEntity DataProductsSummaryEntity { get; set; }

    [JsonProperty("healthActionsSummary")]
    public HealthActionsSummaryEntity HealthActionsSummaryEntity { get; set; }

    [JsonProperty("healthReportsSummary")]
    public HealthReportsSummaryEntity HealthReportsSummaryEntity { get; set; }
}
