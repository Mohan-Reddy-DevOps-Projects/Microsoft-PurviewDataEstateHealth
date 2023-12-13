// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Newtonsoft.Json;

/// <summary>
/// NOTE: Properties of Callback Request Context are serialized along with the Callback messages. So, only select properties are part of the context here.
/// </summary>
public sealed class CallbackRequestContext
{
    private string apiVersion;
    private Guid accountId;
    private Guid tenantId;
    private string catalogId;
    private string accountResourceId;
    private string accountName;
    private PurviewAccountSku skuName;
    private string correlationId;

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
        this.apiVersion = context.ApiVersion;
        this.accountId = context.AccountObjectId;
        this.tenantId = context.TenantId;
        this.catalogId = context.CatalogId;
        this.accountResourceId = context.AccountResourceId;
        this.accountName = context.AccountName;
        this.skuName = context.PurviewAccountSku;
        this.correlationId = context.CorrelationId;
    }

    /// <summary>
    /// The api version.
    /// </summary>
    [JsonProperty(PropertyName = "apiVersion")]
    public string ApiVersion { get => this.apiVersion; set => this.apiVersion = value; }

    /// <summary>
    /// The account id.
    /// </summary>
    [JsonProperty(PropertyName = "accountId")]
    public Guid AccountId { get => this.accountId; set => this.accountId = value; }

    /// <summary>
    /// The tenant id.
    /// </summary>
    [JsonProperty(PropertyName = "tenantId")]
    public Guid TenantId { get => this.tenantId; set => this.tenantId = value; }

    /// <summary>
    /// The catalog id.
    /// </summary>
    [JsonProperty(PropertyName = "catalogId")]
    public string CatalogId { get => this.catalogId; set => this.catalogId = value; }

    /// <summary>
    /// The account name.
    /// </summary>
    [JsonProperty(PropertyName = "accountName")]
    public string AccountName { get => this.accountName; set => this.accountName = value; }

    /// <summary>
    /// The account name.
    /// </summary>
    [JsonProperty(PropertyName = "accountResourceId")]
    public string AccountResourceId { get => this.accountResourceId; set => this.accountResourceId = value; }

    /// <summary>
    /// The account sku.
    /// </summary>
    [JsonProperty(PropertyName = "skuName")]
    public PurviewAccountSku SkuName { get => this.skuName; set => this.skuName = value; }

    /// <summary>
    /// The correlation id.
    /// </summary>
    [JsonProperty(PropertyName = "correlationId")]
    public string CorrelationId { get => this.correlationId; set => this.correlationId = value; }
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
