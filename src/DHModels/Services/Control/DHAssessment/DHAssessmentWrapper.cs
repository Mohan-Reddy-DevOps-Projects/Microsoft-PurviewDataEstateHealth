namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DHAssessmentWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHAssessmentWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyTargetEntityType = "targetEntityType";
    private const string keyTargetQualityType = "targetQualityType";
    private const string keyRules = "rules";
    private const string keyAggregation = "aggregation";
    private const string keySyatemTemplate = "systemTemplate";

    public static DHAssessmentWrapper Create(JObject jObject)
    {
        return new DHAssessmentWrapper(jObject);
    }

    public DHAssessmentWrapper() : this([]) { }

    [EntityProperty(keyName)]
    [EntityRequiredValidator]
    [EntityNameValidator]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyTargetEntityType)]
    [EntityRequiredValidator]
    public DHAssessmentTargetEntityType? TargetEntityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyTargetEntityType);
            return Enum.TryParse<DHAssessmentTargetEntityType>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyTargetEntityType, value?.ToString());
    }

    [EntityProperty(keyTargetQualityType)]
    public DHAssessmentQualityType TargetQualityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyTargetQualityType);
            return Enum.TryParse<DHAssessmentQualityType>(enumStr, true, out var result) ? result : DHAssessmentQualityType.MetadataQuality;
        }
        set => this.SetPropertyValue(keyTargetQualityType, value.ToString());
    }

    private IEnumerable<DHAssessmentRuleWrapper>? rules;

    [EntityProperty(keyRules)]
    public IEnumerable<DHAssessmentRuleWrapper> Rules
    {
        get => this.rules ??= this.GetPropertyValueAsWrappers<DHAssessmentRuleWrapper>(keyRules) ?? [];
        set
        {
            this.SetPropertyValueFromWrappers(keyRules, value);
            this.rules = value;
        }
    }

    private DHAssessmentAggregationBaseWrapper? Aggregation;

    [EntityProperty(keyAggregation)]
    public DHAssessmentAggregationBaseWrapper AggregationWrapper
    {
        get => this.Aggregation ??= this.GetPropertyValueAsWrapper<DHAssessmentAggregationBaseWrapper>(keyAggregation);
        set
        {
            this.SetPropertyValueFromWrapper(keyAggregation, value);
            this.Aggregation = value;
        }
    }

    [EntityProperty(keySyatemTemplate, true)]
    public string SyatemTemplate
    {
        get => this.GetPropertyValue<string>(keySyatemTemplate);
        set => this.SetPropertyValue(keySyatemTemplate, value);
    }

    public override void OnCreate(string userId)
    {
        base.OnCreate(userId);

        this.CheckAssessmentRuleId();
    }

    public override void OnUpdate(DHAssessmentWrapper existWrapper, string userId)
    {
        base.OnUpdate(existWrapper, userId);

        this.SyatemTemplate = existWrapper.SyatemTemplate;

        this.CheckAssessmentRuleId();
    }

    private void CheckAssessmentRuleId()
    {
        foreach (var rule in this.Rules)
        {
            if (string.IsNullOrEmpty(rule.Id))
            {
                rule.Id = Guid.NewGuid().ToString();
            }
        }
    }
}