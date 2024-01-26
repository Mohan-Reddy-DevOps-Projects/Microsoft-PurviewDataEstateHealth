#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;

using Newtonsoft.Json;

public class DHSimpleRule : DHRuleBase
{
    [JsonProperty("operator")]
    public required DHOperator Operator { get; set; }

    /// <summary>
    /// Operand could be null as some operators may have no operands.
    /// </summary>
    [JsonProperty("operand")]
    public string? Operand { get; set; }
}

public enum DHOperator
{
    Equal,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    InRange,
    IsNull,
    IsEmpty,
    RegexMatch,
    IsTrue,
}
