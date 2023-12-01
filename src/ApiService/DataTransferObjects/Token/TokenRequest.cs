// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;

/// <summary>
/// A token request data transfer object.
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Gets or sets the dataset ids.
    /// </summary>
    [JsonProperty("datasetIds")]
    public HashSet<Guid> DatasetIds { get; internal set; }

    /// <summary>
    /// Gets or sets the report ids.
    /// </summary>
    [JsonProperty("reportIds")]
    public HashSet<Guid> ReportIds{ get; internal set; }
}
