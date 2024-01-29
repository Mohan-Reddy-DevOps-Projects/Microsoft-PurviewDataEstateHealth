#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Newtonsoft.Json;

public class DHExpressionRule : DHRuleBase
{
    [JsonProperty("ruleType")]
    public override DHRuleType RuleType
    {
        get => DHRuleType.ExpressionRule;
        set { }
    }

    [JsonProperty("expression")]
    public required string Expression { get; set; }
}
