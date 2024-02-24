// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;

/// <summary>
/// Job Metadata
/// </summary>
public class WorkerJobMetadata
{
    /// <summary>
    /// Gets or sets the request correlation context.
    /// </summary>
    [JsonProperty]
    public RequestCorrelationContext RequestCorrelationContext { get; set; }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    [JsonProperty]
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the subscription id.
    /// </summary>
    [JsonProperty]
    public string SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the error model.
    /// </summary>
    [JsonProperty]
    public ErrorModel ErrorInfo { get; set; }

    /// <summary>
    /// Job Metadata
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return this.ToJson();
    }
}
