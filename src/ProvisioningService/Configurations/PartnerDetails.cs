// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using System.ComponentModel;
using Newtonsoft.Json;

/// <summary>
/// Partner details.
/// </summary>
public class PartnerDetails : IPartnerDetails
{
    /// <inheritdoc />
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <inheritdoc />
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }

    /// <inheritdoc />
    [JsonProperty("validateResponse")]
    public bool ValidateResponse { get; set; }

    /// <inheritdoc />
    [DefaultValue(180)]
    [JsonProperty("createOrUpdateTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
    public int CreateOrUpdateTimeoutSeconds { get; set; }

    /// <inheritdoc />
    [DefaultValue(180)]
    [JsonProperty("deleteTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
    public int DeleteTimeoutSeconds { get; set; }

    /// <inheritdoc />
    [JsonProperty("dependsOn")]
    public string[] DependsOn { get; set; }
}
