namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Models
{
    using Newtonsoft.Json;

    public class PagedResults<T>
    {
        [JsonProperty("value")]
        public IEnumerable<T> Value { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}