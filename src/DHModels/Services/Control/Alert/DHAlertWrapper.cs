namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert.NotifyTarget;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHAlertWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHAlertWrapper>(jObject)
{
    private const string keyRule = "rule";
    private const string keyNotifyTargets = "notifyTargets";
    private const string keyControlIds = "controlIds";

    public static DHAlertWrapper Create(JObject jObject)
    {
        return new DHAlertWrapper(jObject);
    }

    public DHAlertWrapper() : this([]) { }

    private IEnumerable<string>? controlIds;

    [EntityTypeProperty(keyControlIds)]
    public IEnumerable<string>? ControlIds
    {
        get => this.controlIds ??= this.GetPropertyValues<string>(keyControlIds);
        set
        {
            this.SetPropertyValue(keyControlIds, value);
            this.controlIds = value;
        }
    }

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

    private IEnumerable<DHNotifyTargetBaseWrapper>? notifyTargets;

    [EntityProperty(keyNotifyTargets)]
    [EntityRequiredValidator]
    public IEnumerable<DHNotifyTargetBaseWrapper>? NotifyTargets
    {
        get => this.notifyTargets ??= this.GetPropertyValueAsWrappers<DHNotifyTargetBaseWrapper>(keyNotifyTargets);
        set
        {
            this.SetPropertyValueFromWrappers(keyNotifyTargets, value);
            this.notifyTargets = value;
        }
    }

    public override void Validate()
    {
        base.Validate();

        this.Rule.ValidateCheckPoints(DHRuleSourceType.Alert);
    }
}
