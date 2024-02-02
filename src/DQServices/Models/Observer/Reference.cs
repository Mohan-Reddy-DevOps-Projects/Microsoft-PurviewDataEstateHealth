namespace Microsoft.Purview.DataEstateHealth.DHModels.Models.Observer;

using Newtonsoft.Json;

public class Reference
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("referenceId")]
    public string ReferenceId { get; set; }
}
