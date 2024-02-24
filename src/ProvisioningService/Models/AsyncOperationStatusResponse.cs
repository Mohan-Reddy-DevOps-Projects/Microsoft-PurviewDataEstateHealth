// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using Newtonsoft.Json.Converters;

using Newtonsoft.Json;

/// <summary>
/// Response body model for operation status.
/// </summary>
public class AsyncOperationStatusResponse
{
    /// <summary>
    /// Operation creation timestamp in ISO 8601 format.
    /// </summary>
    [JsonProperty(PropertyName = "createdDateTime", Required = Required.Default)]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? CreatedDateTime { get; set; }

    /// <summary>
    /// Operation last modified timestamp in ISO 8601 format.
    /// </summary>
    [JsonProperty(PropertyName = "lastActionDateTime", Required = Required.Default)]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? LastActionDateTime { get; set; }

    /// <summary>
    /// Operation state of the long running operation.
    /// </summary>
    [JsonProperty(PropertyName = "status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public AsyncOperationStatus Status { get; set; }
}
