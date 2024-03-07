namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class DHAssessmentWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHAssessmentWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyTargetEntityType = "targetEntityType";
    private const string keyTargetQualityType = "targetQualityType";
    private const string keyRules = "rules";
    private const string keyAggregation = "aggregation";
    private const string keySystemTemplate = "systemTemplate";
    private const string keySystemTemplateEntityId = "systemTemplateEntityId";

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
    [EntityRequiredValidator]
    public DHAssessmentAggregationBaseWrapper AggregationWrapper
    {
        get => this.Aggregation ??= this.GetPropertyValueAsWrapper<DHAssessmentAggregationBaseWrapper>(keyAggregation);
        set
        {
            this.SetPropertyValueFromWrapper(keyAggregation, value);
            this.Aggregation = value;
        }
    }

    [EntityProperty(keySystemTemplate, true)]
    public string SystemTemplate
    {
        get => this.GetPropertyValue<string>(keySystemTemplate);
        set => this.SetPropertyValue(keySystemTemplate, value);
    }

    [EntityProperty(keySystemTemplateEntityId, true)]
    public string SystemTemplateEntityId
    {
        get => this.GetPropertyValue<string>(keySystemTemplateEntityId);
        set => this.SetPropertyValue(keySystemTemplateEntityId, value);
    }

    public override void OnCreate(string userId, string? id = null)
    {
        base.OnCreate(userId, id);

        this.UpdateAssessmentRuleId();
    }

    public override void OnUpdate(DHAssessmentWrapper existWrapper, string userId)
    {
        base.OnUpdate(existWrapper, userId);

        this.SystemTemplate = existWrapper.SystemTemplate;
        this.SystemTemplateEntityId = existWrapper.SystemTemplateEntityId;

        this.UpdateAssessmentRuleId();
    }

    public override void Validate()
    {
        base.Validate();

        switch (this.TargetQualityType)
        {
            case DHAssessmentQualityType.DataQuality:
                switch (this.TargetEntityType)
                {
                    case DHAssessmentTargetEntityType.DataProduct:
                        foreach (var item in this.Rules)
                        {
                            item.Rule.ValidateCheckPoints(Rule.Helpers.DHRuleSourceType.AssessmentDQDataProduct);
                        }
                        break;
                    case null:
                        if (this.Rules.Any())
                        {
                            throw new EntityValidationException(String.Format(
                                CultureInfo.InvariantCulture,
                                StringResources.ErrorMessageAssessmentRuleShouldBeEmpty
                                ));
                        }
                        break;
                    default:
                        var supportedTargetEntityType = new[] { DHAssessmentTargetEntityType.DataProduct };
                        throw new EntityValidationException(String.Format(
                            CultureInfo.InvariantCulture,
                            StringResources.ErrorMessageUnsupportedTargetEntityType,
                            this.TargetEntityType,
                            this.TargetQualityType,
                            String.Join(", ", supportedTargetEntityType.Select(i => i.ToString()))
                            ));
                }
                break;
            case DHAssessmentQualityType.MetadataQuality:
                switch (this.TargetEntityType)
                {
                    case DHAssessmentTargetEntityType.DataProduct:
                        foreach (var item in this.Rules)
                        {
                            item.Rule.ValidateCheckPoints(Rule.Helpers.DHRuleSourceType.AssessmentMQDataProduct);
                        }
                        break;
                    case DHAssessmentTargetEntityType.DataAsset:
                        foreach (var item in this.Rules)
                        {
                            item.Rule.ValidateCheckPoints(Rule.Helpers.DHRuleSourceType.AssessmentMQDataAsset);
                        }
                        break;
                    case DHAssessmentTargetEntityType.CriticalDataElement:
                        foreach (var item in this.Rules)
                        {
                            item.Rule.ValidateCheckPoints(Rule.Helpers.DHRuleSourceType.AssessmentMQCDE);
                        }
                        break;
                    case DHAssessmentTargetEntityType.BusinessDomain:
                        foreach (var item in this.Rules)
                        {
                            item.Rule.ValidateCheckPoints(Rule.Helpers.DHRuleSourceType.AssessmentMQBusinessDomain);
                        }
                        break;
                    case null:
                        if (this.Rules.Any())
                        {
                            throw new EntityValidationException(String.Format(
                                CultureInfo.InvariantCulture,
                                StringResources.ErrorMessageAssessmentRuleShouldBeEmpty
                            ));
                        }
                        break;
                    default:
                        var supportedTargetEntityType = new[]
                        {
                            DHAssessmentTargetEntityType.DataProduct,
                            DHAssessmentTargetEntityType.DataAsset,
                            DHAssessmentTargetEntityType.CriticalDataElement,
                            DHAssessmentTargetEntityType.BusinessDomain
                        };
                        throw new EntityValidationException(String.Format(
                            CultureInfo.InvariantCulture,
                            StringResources.ErrorMessageUnsupportedTargetEntityType,
                            this.TargetEntityType,
                            this.TargetQualityType,
                            String.Join(", ", supportedTargetEntityType.Select(i => i.ToString()))
                        ));
                }
                break;
        }
    }

    private void UpdateAssessmentRuleId()
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