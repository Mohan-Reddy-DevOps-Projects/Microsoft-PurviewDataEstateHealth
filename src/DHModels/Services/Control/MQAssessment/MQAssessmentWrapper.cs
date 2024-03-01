namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class MQAssessmentWrapper(JObject jObject) : ContainerEntityBaseWrapper<MQAssessmentWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyTargetEntityType = "targetEntityType";
    private const string keyTargetQualityType = "targetQualityType";
    private const string keyRules = "rules";
    private const string keyAggregation = "aggregation";
    private const string keyReserved = "reserved";

    public static MQAssessmentWrapper Create(JObject jObject)
    {
        return new MQAssessmentWrapper(jObject);
    }

    public MQAssessmentWrapper() : this([]) { }

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
    public MQAssessmentTargetEntityType? TargetEntityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyTargetEntityType);
            return Enum.TryParse<MQAssessmentTargetEntityType>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyTargetEntityType, value?.ToString());
    }

    [EntityProperty(keyTargetQualityType)]
    public MQAssessmentQualityType TargetQualityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyTargetQualityType);
            return Enum.TryParse<MQAssessmentQualityType>(enumStr, true, out var result) ? result : MQAssessmentQualityType.MetadataQuality;
        }
        set => this.SetPropertyValue(keyTargetQualityType, value.ToString());
    }

    private IEnumerable<MQAssessmentRuleWrapper>? rules;

    [EntityProperty(keyRules)]
    public IEnumerable<MQAssessmentRuleWrapper> Rules
    {
        get => this.rules ??= this.GetPropertyValueAsWrappers<MQAssessmentRuleWrapper>(keyRules);
        set
        {
            this.SetPropertyValueFromWrappers(keyRules, value);
            this.rules = value;
        }
    }

    private MQAssessmentAggregationBaseWrapper? Aggregation;

    [EntityProperty(keyAggregation)]
    public MQAssessmentAggregationBaseWrapper AggregationWrapper
    {
        get => this.Aggregation ??= this.GetPropertyValueAsWrapper<MQAssessmentAggregationBaseWrapper>(keyAggregation);
        set
        {
            this.SetPropertyValueFromWrapper(keyAggregation, value);
            this.Aggregation = value;
        }
    }

    [EntityProperty(keyReserved, true)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }

    public override void OnUpdate(MQAssessmentWrapper existWrapper, string userId)
    {
        if (existWrapper.Reserved)
        {
            throw new EntityReservedException();
        }

        base.OnUpdate(existWrapper, userId);

        this.Reserved = existWrapper.Reserved;
    }
}