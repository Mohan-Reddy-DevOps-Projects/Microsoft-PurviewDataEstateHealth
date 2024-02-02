#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHRuleOrGroupBaseWrapperDerivedTypes.SimpleRule, EntityCategory.Rule)]
public class DHSimpleRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyOperator = "operator";
    private const string keyOperand = "operand";

    [EntityProperty(keyOperator)]
    [CosmosDBEnumString]
    public DHOperator Operator
    {
        get => this.GetPropertyValue<DHOperator>(keyOperator);
        set => this.SetPropertyValue(keyOperator, value);
    }

    /// <summary>
    /// Operand could be null as some operators may have no operands.
    /// </summary>
    [EntityProperty(keyOperand)]
    public string? Operand
    {
        get => this.GetPropertyValue<string?>(keyOperand);
        set => this.SetPropertyValue(keyOperand, value);
    }
}
