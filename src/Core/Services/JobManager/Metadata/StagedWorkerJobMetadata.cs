// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// Job Metadata
/// </summary>
public class StagedWorkerJobMetadata : JobMetadataBase
{
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
}
