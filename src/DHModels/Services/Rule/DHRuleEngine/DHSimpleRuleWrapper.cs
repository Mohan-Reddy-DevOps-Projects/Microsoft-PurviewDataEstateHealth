namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
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
    [EntityRequiredValidator]
    public DHCheckPoint? CheckPoint
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyCheckPoint);
            return Enum.TryParse<DHCheckPoint>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyCheckPoint, value?.ToString());
    }

    [EntityTypeProperty(keyOperator)]
    [EntityRequiredValidator]
    public DHOperator? Operator
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyOperator);
            return Enum.TryParse<DHOperator>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyOperator, value?.ToString());
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
