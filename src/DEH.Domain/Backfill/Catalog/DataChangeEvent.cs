namespace DEH.Domain.Backfill.Catalog;

using Newtonsoft.Json;

public class DataChangeEvent<T>
{
    [JsonProperty("eventId")] public required string EventId { get; set; }

    [JsonProperty("correlationId")] public required string CorrelationId { get; set; }

    [JsonProperty("eventSource")] public EventSource EventSource { get; set; }

    [JsonProperty("payloadKind")] public PayloadKind PayloadKind { get; set; }

    [JsonProperty("operationType")] public OperationType OperationType { get; set; }

    [JsonProperty("preciseTimestamp")] public required string PreciseTimestamp { get; set; }

    [JsonProperty("tenantId")] public required string TenantId { get; set; }

    [JsonProperty("accountId")] public required string AccountId { get; set; }

    [JsonProperty("changedBy")] public required string ChangedBy { get; set; }

    [JsonProperty("EventEnqueuedUtcTime")] public DateTime EventEnqueuedUtcTime { get; set; }

    [JsonProperty("EventProcessedUtcTime")]
    public DateTime EventProcessedUtcTime { get; set; }

    [JsonProperty("PartitionId")] public int PartitionId { get; set; }

    [JsonProperty("payload")] public required DataChangeEventPayload<T> Payload { get; set; }

    [JsonProperty("id")] public required string Id { get; set; }
}