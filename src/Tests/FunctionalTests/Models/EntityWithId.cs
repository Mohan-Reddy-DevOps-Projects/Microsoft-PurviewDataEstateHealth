namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Models
{
    using Newtonsoft.Json;

    public class EntityWithId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
