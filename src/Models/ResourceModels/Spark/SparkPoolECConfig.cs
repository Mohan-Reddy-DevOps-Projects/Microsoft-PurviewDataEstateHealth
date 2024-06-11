#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;

using Newtonsoft.Json;

public class SparkPoolECConfig
{
    /// <summary>
    /// Gets or sets the max capacity units.
    /// </summary>
    [JsonProperty("maxCapacityUnits")]
    public int? MaxCapacityUnits { get; set; }

    /// <summary>
    /// Gets or sets the node size.
    /// </summary>
    [JsonProperty("nodeSize")]
    public string? NodeSize { get; set; }
}
