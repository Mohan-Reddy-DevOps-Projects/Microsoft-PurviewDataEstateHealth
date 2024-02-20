namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class MQAssessmentWrapper(JObject jObject) : ContainerEntityBaseWrapper<MQAssessmentWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyTargetEntityType = "targetEntityType";
    private const string keyRules = "rules";
    private const string keyAggregation = "aggregation";
    private const string keyReserved = "reserved";

    public static MQAssessmentWrapper Create(JObject jObject)
    {
        return new MQAssessmentWrapper(jObject);
    }

    public MQAssessmentWrapper() : this([]) { }

    [EntityProperty(keyName)]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyTargetEntityType)]
    public MQAssessmentTargetEntityType TargetEntityType
    {
        get => this.GetPropertyValue<MQAssessmentTargetEntityType>(keyTargetEntityType);
        set => this.SetPropertyValue(keyTargetEntityType, value);
    }

    private IEnumerable<DHRuleBaseWrapper>? rules;

    [EntityProperty(keyRules)]
    public IEnumerable<DHRuleBaseWrapper> Rules
    {
        get => this.rules ??= this.GetPropertyValueAsWrappers<DHRuleBaseWrapper>(keyRules);
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

    [EntityProperty(keyReserved)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum MQAssessmentTargetEntityType
{
    DataProduct
}
