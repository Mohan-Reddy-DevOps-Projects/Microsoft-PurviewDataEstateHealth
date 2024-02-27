// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <inheritdoc />
public class MDQJobCallbackPayload
{
    /// <inheritdoc />
    [JsonProperty("dqJobId")]
    public string DQJobId { get; set; }

    /// <inheritdoc />
    [JsonProperty("jobStatus")]
    public string JobStatus { get; set; }
}
