#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public abstract class ContainerEntityBase
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonIgnore]
    [CosmosDBPartitionKey]
    public required Guid TenantId { get; set; }

    [JsonIgnore]
    public required Guid AccountId { get; set; }

    [JsonProperty("auditLogs")]
    public IList<ContainerEntityAuditLog> AuditLogs { get; set; } = new List<ContainerEntityAuditLog>();
}

public class ContainerEntityAuditLog
{
    [JsonProperty("timestamp", Required = Required.Always)]
    public required long Timestamp { get; set; }

    [JsonProperty("user", Required = Required.Always)]
    public required string User { get; set; }

    [JsonProperty("action", Required = Required.Always)]
    [CosmosDBEnumString]
    public required ContainerEntityAuditAction Action { get; set; }
}

public enum ContainerEntityAuditAction
{
    Create,
    Update,
}
