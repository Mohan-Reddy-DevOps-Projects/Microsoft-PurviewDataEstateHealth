namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.SimpleRule, EntityCategory.Rule)]
public class DHSimpleRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyOperator = "operator";
    private const string keyOperand = "operand";
    private const string keyCheckPoint = "checkPoint";

    public DHSimpleRuleWrapper() : this([]) { }

    [EntityTypeProperty(keyCheckPoint)]
    [CosmosDBEnumString]
    public DHCheckPoints CheckPoint
    {
        get => this.GetTypePropertyValue<DHCheckPoints>(keyCheckPoint);
        set => this.SetTypePropertyValue(keyCheckPoint, value);
    }

    [EntityTypeProperty(keyOperator)]
    [CosmosDBEnumString]
    public DHOperator Operator
    {
        get => this.GetTypePropertyValue<DHOperator>(keyOperator);
        set => this.SetTypePropertyValue(keyOperator, value);
    }

    /// <summary>
    /// Operand could be null as some operators may have no operands.
    /// </summary>
    [EntityTypeProperty(keyOperand)]
    public string? Operand
    {
        get => this.GetTypePropertyValue<string?>(keyOperand);
        set => this.SetTypePropertyValue(keyOperand, value);
    }
}
