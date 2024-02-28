// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------


namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

[JsonConverter(typeof(StringEnumConverter))]
public enum DataHealthActionCategory
{
    HealthControl,
    DataQuality
}

//TODO: need confirmdation from PM
[JsonConverter(typeof(StringEnumConverter))]
public enum DataHealthActionStatus
{
    NotStarted,
    InProgress,
    Resolved,
    Ignored
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DataHealthActionSeverity
{
    High,
    Medium,
    Low
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DataHealthActionTargetEntityType
{
    BusinessDomain,
    DataProduct,
    GlossaryTerm,
    DataAsset,
    DataQualityAccessment
}

[EntityWrapper(EntityCategory.Action)]
public class DataHealthActionWrapper(JObject jObject) : ContainerEntityBaseWrapper<DataHealthActionWrapper>(jObject)
{
    public const string keyFindingType = "findingType";
    public const string keyFindingSubType = "findingSubType";
    public const string keyFindingName = "findingName";
    public const string keySeverity = "severity";
    private const string keyId = "id";
    private const string keyCategory = "category";
    private const string keyFindingId = "findingId";
    private const string keyRecommendation = "recommendation";
    private const string keyDomainId = "domainId";
    private const string keyStatus = "status";
    private const string keyTargetEntityType = "targetEntityType";
    private const string keyTargetEntityId = "targetEntityId";
    private const string keyAssignedTo = "assignedTo";
    private const string keySystemData = "systemData";
    private const string KeyReason = "reason";
    private const string KeyExtraProperties = "extraProperties";

    public static DataHealthActionWrapper Create(JObject jObject)
    {
        return new DataHealthActionWrapper(jObject);
    }
    public DataHealthActionWrapper() : this([]) { }

    [EntityProperty(keyCategory)]
    [EntityRequiredValidator]
    public DataHealthActionCategory Category
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyCategory);
            return Enum.TryParse<DataHealthActionCategory>(enumStr, true, out var result) ? result : DataHealthActionCategory.HealthControl;
        }
        set => this.SetPropertyValue(keyCategory, value.ToString());
    }

    [EntityProperty(keyFindingType)]
    [EntityRequiredValidator]
    public string FindingType
    {
        get => this.GetPropertyValue<string>(keyFindingType);
        set => this.SetPropertyValue(keyFindingType, value);
    }

    [EntityProperty(keyFindingSubType)]
    [EntityRequiredValidator]
    public string FindingSubType
    {
        get => this.GetPropertyValue<string>(keyFindingSubType);
        set => this.SetPropertyValue(keyFindingSubType, value);
    }

    [EntityProperty(keyFindingName)]
    [EntityRequiredValidator]
    public string FindingName
    {
        get => this.GetPropertyValue<string>(keyFindingName);
        set => this.SetPropertyValue(keyFindingName, value);
    }

    [EntityProperty(keyFindingId)]
    [EntityRequiredValidator]
    public string FindingId
    {
        get => this.GetPropertyValue<string>(keyFindingId);
        set => this.SetPropertyValue(keyFindingId, value);
    }

    [EntityProperty(keyRecommendation)]
    [EntityRequiredValidator]
    public string Recommendation
    {
        get => this.GetPropertyValue<string>(keyRecommendation);
        set => this.SetPropertyValue(keyRecommendation, value);
    }

    [EntityProperty(keyDomainId)]
    [EntityRequiredValidator]
    public string DomainId
    {
        get => this.GetPropertyValue<string>(keyDomainId);
        set => this.SetPropertyValue(keyDomainId, value);
    }

    [EntityProperty(keyStatus)]
    [EntityRequiredValidator]
    public DataHealthActionStatus Status
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyStatus);
            return Enum.TryParse<DataHealthActionStatus>(enumStr, true, out var result) ? result : DataHealthActionStatus.NotStarted;
        }
        set => this.SetPropertyValue(keyStatus, value.ToString());
    }

    [EntityProperty(keySeverity)]
    [EntityRequiredValidator]
    public DataHealthActionSeverity Severity
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keySeverity);
            return Enum.TryParse<DataHealthActionSeverity>(enumStr, true, out var result) ? result : DataHealthActionSeverity.Medium;
        }
        set => this.SetPropertyValue(keySeverity, value.ToString());
    }

    [EntityProperty(keyTargetEntityType)]
    [EntityRequiredValidator]
    public DataHealthActionTargetEntityType TargetEntityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyTargetEntityType);
            return Enum.TryParse<DataHealthActionTargetEntityType>(enumStr, true, out var result) ? result : DataHealthActionTargetEntityType.DataProduct;
        }
        set => this.SetPropertyValue(keyTargetEntityType, value.ToString());
    }

    [EntityProperty(keyTargetEntityId)]
    [EntityRequiredValidator]
    public string TargetEntityId
    {
        get => this.GetPropertyValue<string>(keyTargetEntityId);
        set => this.SetPropertyValue(keyTargetEntityId, value);
    }

    private IEnumerable<string>? assignedTo;

    [EntityProperty(keyAssignedTo)]
    public IEnumerable<string> AssignedTo
    {
        get => this.assignedTo ??= (this.GetPropertyValues<string>(keyAssignedTo) ?? []);
        set
        {
            this.SetPropertyValue(keyAssignedTo, value);
            this.assignedTo = value;
        }
    }

    private ActionSystemDataWrapper? systemData;

    [EntityProperty(keySystemData, true)]
    public ActionSystemDataWrapper SystemInfo
    {
        get
        {
            this.systemData ??= this.GetPropertyValueAsWrapper<ActionSystemDataWrapper>(keySystemData);
            return this.systemData;
        }
        set
        {
            this.SetPropertyValueFromWrapper(keySystemData, value);
            this.systemData = value;
        }
    }

    [EntityProperty(KeyReason)]
    public string Reason
    {
        get => this.GetPropertyValue<string>(KeyReason);
        set => this.SetPropertyValue(KeyReason, value);
    }

    private ActionExtraPropertiesWrapper? extraProperties;

    [EntityProperty(KeyExtraProperties)]
    public ActionExtraPropertiesWrapper ExtraProperties
    {
        get
        {
            this.extraProperties ??= this.GetPropertyValueAsWrapper<ActionExtraPropertiesWrapper>(KeyExtraProperties);
            return this.extraProperties;
        }
        set
        {
            this.SetPropertyValueFromWrapper(KeyExtraProperties, value);
            this.extraProperties = value;
        }
    }

    public override void OnCreate(string userId)
    {
        base.OnCreate(userId);

        this.SystemInfo = new ActionSystemDataWrapper(DateTime.UtcNow);
    }

    public override void OnUpdate(DataHealthActionWrapper existed, string userId)
    {
        base.OnUpdate(existed, userId);

        this.SystemInfo = existed.SystemInfo;
        this.SystemInfo.OnModify(userId);
    }
}
