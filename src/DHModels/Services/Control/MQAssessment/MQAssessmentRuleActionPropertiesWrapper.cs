namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;

public class MQAssessmentRuleActionPropertiesWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyActionName = "actionName";
    private const string keyActionSeverity = "actionSeverity";
    private const string keyActionRecommendation = "actionRecommendation";

    public MQAssessmentRuleActionPropertiesWrapper() : this([]) { }

    [EntityProperty(keyActionName)]
    public string ActionName
    {
        get => this.GetPropertyValue<string>(keyActionName);
        set => this.SetPropertyValue(keyActionName, value);
    }

    [EntityProperty(keyActionSeverity)]
    public DataHealthActionSeverity? Severity
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyActionSeverity);
            return Enum.TryParse<DataHealthActionSeverity>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyActionSeverity, value?.ToString());
    }

    [EntityProperty(keyActionRecommendation)]
    public string Recommendation
    {
        get => this.GetPropertyValue<string>(keyActionRecommendation);
        set => this.SetPropertyValue(keyActionRecommendation, value);
    }
}
