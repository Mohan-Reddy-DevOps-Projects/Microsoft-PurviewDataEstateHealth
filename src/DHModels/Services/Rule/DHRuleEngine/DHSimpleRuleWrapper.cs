namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(DHRuleBaseWrapperDerivedTypes.SimpleRule, EntityCategory.Rule)]
public class DHSimpleRuleWrapper(JObject jObject) : DHRuleBaseWrapper(jObject)
{
    private const string keyOperator = "operator";
    private const string keyOperand = "operand";
    private const string keyCheckPoint = "checkPoint";

    public DHSimpleRuleWrapper() : this([]) { }

    [EntityTypeProperty(keyCheckPoint)]
    public DHCheckPoints? CheckPoint
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyCheckPoint);
            return Enum.TryParse<DHCheckPoints>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyCheckPoint, value?.ToString());
    }

    [EntityTypeProperty(keyOperator)]
    public DHOperator? Operator
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyOperator);
            return Enum.TryParse<DHOperator>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyOperator, value?.ToString());
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
