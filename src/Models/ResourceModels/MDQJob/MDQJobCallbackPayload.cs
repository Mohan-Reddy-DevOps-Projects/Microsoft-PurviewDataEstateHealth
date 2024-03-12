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
    public Guid DQJobId { get; set; }

    /// <inheritdoc />
    [JsonProperty("jobStatus")]
    public string JobStatus { get; set; }

    /// <inheritdoc />
    [JsonProperty("tenantId")]
    public Guid TenantId { get; set; }

    /// <inheritdoc />
    [JsonProperty("accountId")]
    public Guid AccountId { get; set; }

    /// <inheritdoc />
    [JsonProperty("isRetry")]
    public bool IsRetry { get; set; }

}
