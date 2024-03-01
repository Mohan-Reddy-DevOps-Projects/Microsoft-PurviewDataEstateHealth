namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;

public class DHAssessmentRuleWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyId = "id";
    private const string keyRule = "rule";
    private const string keyActionProperties = "actionProperties";

    public DHAssessmentRuleWrapper() : this([]) { }

    [EntityProperty(keyId)]
    public string Id
    {
        get => this.GetPropertyValue<string>(keyId);
        set => this.SetPropertyValue(keyId, value);
    }

    private DHRuleBaseWrapper? rule;

    [EntityProperty(keyRule)]
    public DHRuleBaseWrapper Rule
    {
        get => this.rule ??= this.GetPropertyValueAsWrapper<DHRuleBaseWrapper>(keyRule);
        set
        {
            this.SetPropertyValueFromWrapper(keyRule, value);
            this.rule = value;
        }
    }

    private DHAssessmentRuleActionPropertiesWrapper? actionProperties;

    [EntityProperty(keyActionProperties)]
    public DHAssessmentRuleActionPropertiesWrapper ActionProperties
    {
        get => this.actionProperties ??= this.GetPropertyValueAsWrapper<DHAssessmentRuleActionPropertiesWrapper>(keyActionProperties);
        set
        {
            this.SetPropertyValueFromWrapper(keyActionProperties, value);
            this.actionProperties = value;
        }
    }
}
