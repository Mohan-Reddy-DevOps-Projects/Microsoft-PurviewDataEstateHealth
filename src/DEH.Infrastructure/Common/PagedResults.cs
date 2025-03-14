namespace DEH.Infrastructure.Common;

using Newtonsoft.Json;

public class PagedResults<T>
{
    [JsonProperty("value", Required = Required.Always)]
    public required IEnumerable<T> Value { get; set; }

    [JsonProperty("nextLink", NullValueHandling = NullValueHandling.Ignore)]
    public string? NextLink { get; set; }

    [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
    public int? Count { get; set; }
}