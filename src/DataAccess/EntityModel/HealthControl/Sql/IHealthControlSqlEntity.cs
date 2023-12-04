// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal interface IHealthControlSqlEntity
{
    [JsonProperty("currentScore")]
    public double CurrentScore { get; set; }

    [JsonProperty("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
