// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    public Guid AccountId { get; set; }

    /// <summary>
    /// Event Id.
    /// </summary>
    [JsonProperty("eventId")]
    public Guid EventId { get; set; }

    /// <summary>
    /// Correlation Id.
    /// </summary>
    [JsonProperty("correlationId")]
    public Guid EventCorrelationId { get; set; }

    /// <summary>
    /// Precise timestamp.
    /// </summary>
    [JsonProperty("preciseTimestamp")]
    public DateTime EventCreationTimestamp { get; set; }
}

/// <summary>
/// The access, catalog and quality source model for events.
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
    public Guid TenantId { get; set; }

    /// <summary>
    /// For create, before is null
    /// For delete, after is null
    /// </summary>
    [JsonProperty("payload")]
    public EventPayload Payload { get; set; }

    /// <summary>
    /// Payload format.
    /// </summary>
    [JsonProperty("payloadType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public PayloadFormat PayloadFormat { get; set; }

    /// <summary>
    /// Data Quality flattened payload details
    /// </summary>
    [JsonProperty("payloadDetails")]
    public JObject AlternatePayload { get; set; }

    /// <summary>
    /// Person object id who create/update/delete
    /// </summary>
    [JsonProperty("changedBy")]
    public Guid? ChangedBy { get; set; }
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
    /// Upsert
    /// </summary>
    Upsert,

    /// <summary>
    /// Delete
    /// </summary>
    Delete,

    /// <summary>
    /// Failure
    /// </summary>
    Failure
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
    DataSubscription,

    /// <summary>
    /// Data Quality Fact
    /// </summary>
    DataQualityFact,

    /// <summary>
    /// Data Quality score
    /// </summary>
    DataQualityScore,

    /// <summary>
    /// No OKR events published currently.
    /// </summary>
    OKR,

    /// <summary>
    /// No critical data element events published currently.
    /// </summary>
    CriticalDataElement,

    /// <summary>
    /// No critical data column events published currently.
    /// </summary>
    CriticalDataColumn,

    /// <summary>
    /// Custom metadata events
    /// </summary>
    CustomMetadata,

    /// <summary>
    /// Attribute events
    /// </summary>
    Attribute,

    /// <summary>
    /// Attribute instance events
    /// </summary>
    AttributeInstance
}

/// <summary>
/// Payload Format.
/// </summary>
public enum PayloadFormat
{
    /// <summary>
    /// Inline
    /// </summary>
    Inline = 1,

    /// <summary>
    /// Callback
    /// </summary>
    Callback
}
