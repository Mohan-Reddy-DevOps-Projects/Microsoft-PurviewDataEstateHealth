namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

    public void UpdateActionProperties()
    {
        
        if (this.Rule != null)
        {
            var setDefaultSeverity = false;
            if (this.ActionProperties == null)
            {
                this.ActionProperties = new DHAssessmentRuleActionPropertiesWrapper();
                setDefaultSeverity = true;
            }
            var ruleProperties = this.Rule.TypeProperties;
            if (ruleProperties != null)
            {
                string checkPoint = ruleProperties["checkPoint"]?.ToString() ?? "";
                string operatorValue = ruleProperties["operator"]?.ToString() ?? "";
                string operand = ruleProperties["operand"]?.ToString() ?? "";
                var actionPropertiesDescription = DHAssessmentRuleConstants.CheckpointDescription.GetValueOrReturnKey(checkPoint, operatorValue);
                var oppositeOperator = DHAssessmentRuleConstants.OppositeOperators.GetValueOrReturnKey(operatorValue);

                var name = actionPropertiesDescription.Name
                        .Replace("{oppositeOperator}", oppositeOperator)
                        .Replace("{operand}", operand).Trim(); 
                var reason = actionPropertiesDescription.Reason
                        .Replace("{oppositeOperator}", oppositeOperator)
                        .Replace("{operand}", operand).Trim();
                var recommendation = actionPropertiesDescription.Recommendation
                        .Replace("{operator}", DHAssessmentRuleConstants.Operators.GetValueOrReturnKey(operatorValue))
                        .Replace("{operand}", operand).Trim(); 
                this.ActionProperties.Name = name;
                this.ActionProperties.Reason = reason;
                this.ActionProperties.Recommendation = recommendation;
                if (setDefaultSeverity)
                {
                    this.ActionProperties.Severity = actionPropertiesDescription.Severity;
                }
                
            }
        }
    }

}
