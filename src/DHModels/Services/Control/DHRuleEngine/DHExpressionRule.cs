#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;

using Newtonsoft.Json;

public class DHExpressionRule : DHRuleBase
{
    [JsonProperty("expression")]
    public required string Expression { get; set; }
}
