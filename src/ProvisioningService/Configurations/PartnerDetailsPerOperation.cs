// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using Newtonsoft.Json;
using System.ComponentModel;
internal sealed class PartnerDetailsPerOperation : PartnerDetails, IPartnerDetailsPerOperation
{
    /// <inheritdoc />
    [JsonProperty("operationType")]
    public OperationType OperationType { get; set; }

    /// <inheritdoc />
    [JsonProperty("httpVerb")]
    public string HttpVerb { get; set; }

    /// <inheritdoc />
    [DefaultValue(180)]
    [JsonProperty("timeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
    public int TimeoutSeconds { get; set; }

    /// <inheritdoc />
    [DefaultValue(300)]
    [JsonProperty("pollingTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
    public int PollingTimeoutSeconds { get; set; }

    /// <inheritdoc />
    [DefaultValue(30)]
    [JsonProperty("pollingIntervalSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
    public int PollingIntervalSeconds { get; set; }
}
