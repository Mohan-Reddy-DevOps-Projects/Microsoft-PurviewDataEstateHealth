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
    Active,
    Resolved
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
            return Enum.TryParse<DataHealthActionStatus>(enumStr, true, out var result) ? result : DataHealthActionStatus.Active;
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

    [EntityProperty(keyAssignedTo)]
    public IList<string> AssignedTo
    {
        get => this.GetPropertyValue<IList<string>>(keyAssignedTo);
        set => this.SetPropertyValue(keyAssignedTo, value);
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

    public void onCreate(DateTime? createAt = null)
    {
        this.SystemInfo = new ActionSystemDataWrapper(DateTime.UtcNow);
    }

    public void OnReplace(DataHealthActionWrapper existed)
    {
        Ensure.IsNotNull(existed, nameof(existed));
        this.Id = existed.Id;
        this.SystemInfo = existed.SystemInfo;
        this.AccountId = existed.AccountId;
        this.TenantId = existed.TenantId;
        this.AuditLogs = existed.AuditLogs;
    }
}
