// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Newtonsoft.Json;

/// <summary>
/// Additional info of errors returned from the service.
/// </summary>
public class ErrorModel
{
    /// <summary>
    /// Constructor for the <see cref="ErrorModel"/> type.
    /// </summary>
    public ErrorModel(string code, string message)
    {
        this.Code = code;
        this.Message = message;
    }

    /// <summary>
    /// The error code.
    /// </summary>
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    public string Code { get; set; }

    /// <summary>
    /// The error message.
    /// </summary>
    [JsonProperty(PropertyName = "message", Required = Required.Always)]
    public string Message { get; set; }

    /// <summary>
    /// The target of the error.
    /// </summary>
    [JsonProperty(PropertyName = "target", NullValueHandling = NullValueHandling.Ignore)]
    public string Target { get; set; }

    /// <summary>
    /// Nested details of the error.
    /// </summary>
    [JsonProperty(PropertyName = "details", NullValueHandling = NullValueHandling.Ignore)]
    public ErrorModel[] Details { get; set; }
}
