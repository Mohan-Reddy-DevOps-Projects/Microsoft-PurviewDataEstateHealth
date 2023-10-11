// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Newtonsoft.Json;

/// <summary>
/// An error returned from the service.
/// </summary>
public class DataEstateHealthError
{
    /// <summary>
    /// The error body.
    /// </summary>
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public DataEstateHealthErrorInfo Error { get; set; }
}
