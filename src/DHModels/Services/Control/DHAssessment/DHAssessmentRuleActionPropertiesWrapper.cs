namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;

public class DHAssessmentRuleActionPropertiesWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyName = "name";
    private const string keySeverity = "severity";
    private const string keyReason = "reason";
    private const string keyRecommendation = "recommendation";

    public DHAssessmentRuleActionPropertiesWrapper() : this([]) { }

    [EntityProperty(keyName)]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keySeverity)]
    public DataHealthActionSeverity? Severity
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keySeverity);
            return Enum.TryParse<DataHealthActionSeverity>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keySeverity, value?.ToString());
    }

    [EntityProperty(keyRecommendation)]
    public string Recommendation
    {
        get => this.GetPropertyValue<string>(keyRecommendation);
        set => this.SetPropertyValue(keyRecommendation, value);
    }

    [EntityProperty(keyReason)]
    public string Reason
    {
        get => this.GetPropertyValue<string>(keyReason);
        set => this.SetPropertyValue(keyReason, value);
    }
}
