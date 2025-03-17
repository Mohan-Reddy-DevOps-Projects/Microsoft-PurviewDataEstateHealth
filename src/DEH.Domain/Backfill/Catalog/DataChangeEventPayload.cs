namespace DEH.Domain.Backfill.Catalog;

using Newtonsoft.Json;

public class DataChangeEventPayload<T>
{
    [JsonProperty("before")]
    public T? Before { get; set; }

    [JsonProperty("after")]
    public T? After { get; set; }

    [JsonProperty("related")]
    public object? Related { get; set; }
}