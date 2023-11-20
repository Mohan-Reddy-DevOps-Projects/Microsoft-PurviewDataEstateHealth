// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The catalog source model for events.
/// </summary>
public abstract class BaseEventHubModel
{
    /// <summary>
    /// Account Id.
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// Event Id.
    /// </summary>
    [JsonProperty("eventId")]
    public string EventId { get; set; }

    /// <summary>
    /// Correlation Id.
    /// </summary>
    [JsonProperty("correlationId")]
    public string EventCorrelationId { get; set; }

    /// <summary>
    /// Precise timestamp.
    /// </summary>
    [JsonProperty("preciseTimestamp")]
    public DateTime EventCreationTimestamp { get; set; }
}

/// <summary>
/// The catalog source model for events.
/// </summary>
public class EventHubModel : BaseEventHubModel
{
    /// <summary>
    /// Source of event.
    /// </summary>
    [JsonProperty("eventSource")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventSource EventSource { get; set; }

    /// <summary>
    /// Payload kind.
    /// </summary>
    [JsonProperty("payloadKind")]
    [JsonConverter(typeof(StringEnumConverter))]
    public PayloadKind PayloadKind { get; set; }

    /// <summary>
    /// Operation type.
    /// </summary>
    [JsonProperty("operationType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventOperationType OperationType { get; set; }

    /// <summary>
    /// Tenant Id.
    /// </summary>
    [JsonProperty("tenantId")]
    public string TenantId { get; set; }

    /// <summary>
    /// For create, before is null
    /// For delete, after is null
    /// </summary>
    [JsonProperty("payload")]
    public EventPayload Payload { get; set; }

    /// <summary>
    /// Person object id who create/update/delete
    /// </summary>
    [JsonProperty("changedBy")]
    public string ChangedBy { get; set; }
}

/// <summary>
/// Event Source Enum.
/// </summary>
public enum EventSource
{
    /// <summary>
    /// Data Catalog
    /// </summary>
    DataCatalog = 1,

    /// <summary>
    /// Data Access
    /// </summary>
    DataAccess
}

/// <summary>
/// Operation Type Enum.
/// </summary>
public enum EventOperationType
{
    /// <summary>
    /// Create
    /// </summary>
    Create = 1,

    /// <summary>
    /// Update
    /// </summary>
    Update,

    /// <summary>
    /// Delete
    /// </summary>
    Delete
}

/// <summary>
/// Event payload.
/// </summary>
public class EventPayload
{
    /// <summary>
    /// Before.
    /// </summary>
    [JsonProperty("before")]
    public JObject Before { get; protected set; }

    /// <summary>
    /// After.
    /// </summary>
    [JsonProperty("after")]
    public JObject After { get; protected set; }
}

/// <summary>
/// Payload Kind Enum.
/// </summary>
public enum PayloadKind
{
    /// <summary>
    /// Data Product
    /// </summary>
    DataProduct = 1,

    /// <summary>
    /// Business Domain
    /// </summary>
    BusinessDomain,

    /// <summary>
    /// Term
    /// </summary>
    Term,

    /// <summary>
    /// Relationship
    /// </summary>
    Relationship,

    /// <summary>
    /// Data Asset
    /// </summary>
    DataAsset,

    /// <summary>
    /// Data Access Policy Set
    /// </summary>
    PolicySet,

    /// <summary>
    /// Data Subscription
    /// </summary>
    DataSubscription
}
