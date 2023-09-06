// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// The purview share error body model.
/// </summary>
public class DataEstateHealthErrorInfoModel
{
    /// <summary>
    /// Instantiate instance of PurviewShareErrorInfoModel.
    /// </summary>
    public DataEstateHealthErrorInfoModel(string code, string message)
    {
        this.Code = code;
        this.Message = message;
    }

    /// <summary>
    /// Code of the error
    /// </summary>
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    public string Code { get; set; }

    /// <summary>
    /// Message of the error
    /// </summary>
    [JsonProperty(PropertyName = "message", Required = Required.Always)]
    public string Message { get; set; }

    /// <summary>
    /// Target of the error
    /// </summary>
    [JsonProperty(PropertyName = "target")]
    public string Target { get; set; }

    /// <summary>
    /// Nested details of the error model
    /// </summary>
    [JsonProperty(PropertyName = "details")]
    public DataEstateHealthErrorInfoModel[] Details { get; set; }
}
