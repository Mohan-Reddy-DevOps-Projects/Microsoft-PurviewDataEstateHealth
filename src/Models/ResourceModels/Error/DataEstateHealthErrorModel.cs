// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// The purview share error model.
/// </summary>
public class DataEstateHealthErrorModel
{
    /// <summary>
    /// The purview share error body
    /// </summary>
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public DataEstateHealthErrorInfoModel Error { get; set; }
}
