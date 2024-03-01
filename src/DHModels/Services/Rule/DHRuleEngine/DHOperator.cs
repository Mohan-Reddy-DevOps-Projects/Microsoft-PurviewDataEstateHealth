namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    IsNullOrEmpty,
    IsNotNullOrEmpty,

    IsTrue,
    IsFalse,

    Normalize,

    // Not supported for PuP
    //StartsWith,
    //EndsWith,
    //InRange,
    //RegexMatch,
    //IsTrue,
    //Contains,
}
