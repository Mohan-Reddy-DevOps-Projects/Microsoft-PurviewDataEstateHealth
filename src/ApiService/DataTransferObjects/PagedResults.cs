// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Responses for list operations.
/// </summary>
/// <typeparam name="TDataTransferObject"></typeparam>
public class PagedResults<TDataTransferObject>
{
    /// <summary>
    /// Collection of items of type DataTransferObjects.
    /// </summary>
    [JsonProperty("value", Required = Required.Always)]
    public ICollection<TDataTransferObject> Value { get; set; }

    /// <summary>
    /// The Url of next result page.
    /// </summary>
    [JsonProperty("nextLink", NullValueHandling = NullValueHandling.Ignore)]
    public string NextLink { get; set; }
}
