// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class HealthActionsSummaryEntity
{
   public HealthActionsSummaryEntity()
   {

   }

   public HealthActionsSummaryEntity(HealthActionsSummaryEntity entity)
   {
        this.HealthActionsTrendLink = entity.HealthActionsTrendLink;
        this.HealthActionsLastRefreshDate = entity.HealthActionsLastRefreshDate;
        this.TotalDismissedActionsCount = entity.TotalDismissedActionsCount;
        this.TotalCompletedActionsCount = entity.TotalCompletedActionsCount;
        this.TotalOpenActionsCount = entity.TotalOpenActionsCount;
   }

    [JsonProperty("totalOpenActionsCount")]
    public int TotalOpenActionsCount { get; set; }

    [JsonProperty("totalCompletedActionsCount")]
    public int TotalCompletedActionsCount { get; set; }

    [JsonProperty("totalDismissedActionsCount")]
    public int TotalDismissedActionsCount { get; set; }

    [JsonProperty("healthActionsTrendLink")]
    public string HealthActionsTrendLink { get; set; }

    [JsonProperty("healthActionsLastRefreshDate")]
    public DateTime HealthActionsLastRefreshDate { get; set; }
}
