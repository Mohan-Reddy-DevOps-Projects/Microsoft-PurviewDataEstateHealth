// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Properties of a Legacy health report
/// </summary>
public class LegacyHealthReportProperties : HealthReportProperties
{
    /// <summary>
    /// Embed Link 
    /// </summary>
    [JsonProperty("embedLink")]
    public string EmbedLink { get; set; }
}
