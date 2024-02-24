// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

/// <summary>
/// Job Metadata
/// </summary>
public class JobMetadataBase : WorkerJobMetadata
{
    /// <summary>
    /// Gets or sets the request correlation context.
    /// </summary>
    [JsonProperty]
    public CallbackRequestContext RequestContext { get; set; }
}
