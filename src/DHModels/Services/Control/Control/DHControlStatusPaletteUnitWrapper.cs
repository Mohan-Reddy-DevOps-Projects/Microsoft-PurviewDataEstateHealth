namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

public class DHControlStatusPaletteUnitWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyRule = "rule";
    private const string keyStatusPaletteId = "statusPaletteId";

    public DHControlStatusPaletteUnitWrapper() : this([]) { }

    private DHRuleBaseWrapper? rule;

    [EntityProperty(keyRule)]
    [EntityRequiredValidator]
    public DHRuleBaseWrapper Rule
    {
        get => this.rule ?? this.GetPropertyValueAsWrapper<DHRuleBaseWrapper>(keyRule);
        set
        {
            this.SetPropertyValueFromWrapper(keyRule, value);
            this.rule = value;
        }
    }

    [EntityProperty(keyStatusPaletteId)]
    [EntityRequiredValidator]
    public string StatusPaletteId
    {
        get => this.GetPropertyValue<string>(keyStatusPaletteId);
        set => this.SetPropertyValue(keyStatusPaletteId, value);
    }
}
