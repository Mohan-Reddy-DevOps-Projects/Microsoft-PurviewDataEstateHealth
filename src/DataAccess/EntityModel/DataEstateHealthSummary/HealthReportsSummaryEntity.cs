// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class HealthReportsSummaryEntity
{
    public HealthReportsSummaryEntity()
    {

    }

    public HealthReportsSummaryEntity(HealthReportsSummaryEntity entity)
    {
        this.DraftReportsCount = entity.DraftReportsCount;
        this.ActiveReportsCount = entity.ActiveReportsCount;
        this.TotalReportsCount = entity.TotalReportsCount;
    }

    [JsonProperty("totalReportsCount")]
    public int TotalReportsCount { get; set; }

    [JsonProperty("activeReportsCount")]
    public int ActiveReportsCount { get; set; }

    [JsonProperty("draftReportsCount")]
    public int DraftReportsCount { get; set; }
}
