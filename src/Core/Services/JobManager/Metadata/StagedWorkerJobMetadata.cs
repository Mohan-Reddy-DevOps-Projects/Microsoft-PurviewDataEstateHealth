// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// NOTE: Properties of Callback Request Context are serialized along with the Callback messages. So, only select properties are part of the context here.
/// </summary>
public sealed class CallbackRequestContext
{
    /// <summary>
    /// For Serialization.
    /// </summary>
    public CallbackRequestContext()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public CallbackRequestContext(IRequestHeaderContext context)
    {
        // Initialize instance variables from context.
        this.ApiVersion = context.ApiVersion;
        this.AccountId = context.AccountObjectId;
        this.TenantId = context.TenantId;
        this.CatalogId = context.CatalogId;
        this.AccountResourceId = context.AccountResourceId;
        this.AccountName = context.AccountName;
        this.SkuName = context.PurviewAccountSku;
        this.CorrelationId = context.CorrelationId;
    }

    /// <summary>
    /// The api version.
    /// </summary>
    [JsonProperty(PropertyName = "apiVersion")]
    public string ApiVersion { get; set; }

    /// <summary>
    /// The account id.
    /// </summary>
    [JsonProperty(PropertyName = "accountId")]
    public Guid AccountId { get; set; }

    /// <summary>
    /// The tenant id.
    /// </summary>
    [JsonProperty(PropertyName = "tenantId")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// The catalog id.
    /// </summary>
    [JsonProperty(PropertyName = "catalogId")]
    public string CatalogId { get; set; }

    /// <summary>
    /// The account name.
    /// </summary>
    [JsonProperty(PropertyName = "accountName")]
    public string AccountName { get; set; }

    /// <summary>
    /// The account name.
    /// </summary>
    [JsonProperty(PropertyName = "accountResourceId")]
    public string AccountResourceId { get; set; }

    /// <summary>
    /// The account sku.
    /// </summary>
    [JsonProperty(PropertyName = "skuName")]
    public PurviewAccountSku SkuName { get; set; }

    /// <summary>
    /// The correlation id.
    /// </summary>
    [JsonProperty(PropertyName = "correlationId")]
    public string CorrelationId { get; set; }
}

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
