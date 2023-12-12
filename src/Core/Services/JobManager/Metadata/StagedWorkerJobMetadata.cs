// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;

/// <summary>
/// Job Metadata
/// </summary>
public class StagedWorkerJobMetadata
{
    /// <summary>
    /// Gets or sets the request correlation context.
    /// </summary>
    [JsonProperty]
    public RequestHeaderContext RequestHeaderContext { get; set; }

    /// <summary>
    /// The distributed root trace ID
    /// </summary>
    [JsonProperty]
    public string RootTraceId { get; set; }

    /// <summary>
    /// Distributed trace ID
    /// </summary>
    [JsonProperty]
    public string TraceId { get; set; }

    /// <summary>
    /// Span ID for distributed tracing
    /// </summary>
    [JsonProperty]
    public string SpanId { get; set; }

    /// <summary>
    /// Service Exceptions thrown for the job.
    /// </summary>
    [JsonProperty]
    public List<ServiceException> ServiceExceptions { get; set; }

    /// <summary>
    /// The context the job executes within.
    /// </summary>
    [JsonProperty]
    public WorkerJobExecutionContext WorkerJobExecutionContext { get; set; }

    /// <summary>
    /// A temporary flag to transition job metadata.
    /// </summary>
    [DefaultValue(false)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool IsContextEnabled { get; set; } = true;

    /// <summary>
    /// Job Metadata
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return this.ToJson();
    }
}
