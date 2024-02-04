// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------


namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

public enum DataHealthActionCategory
{
    HealthControl,
    DataQuality
}

//TODO: need confirmdation from PM
public enum DataHealthActionStatus
{
    Active,
    Resolved
}

public enum DataHealthActionSeverity
{
    High,
    Medium,
    Low
}

public enum DataHealthActionTargetEntityType
{
    BusinessDomain,
    DataProduct,
    GlossaryTerm,
    DataAsset,
    DataQualityAccessment
}

[CosmosDBContainer("DHAction")]
[EntityWrapper(EntityCategory.Action)]
public class DataHealthActionWrapper(JObject jObject) : ContainerEntityBaseWrapper(jObject)
{

    private const string keyId = "id";
    private const string keyCategory = "category";
    private const string keyFindingType = "findingType";
    private const string keyFindingSubType = "findingSubType";
    private const string keyFindingName = "findingName";
    private const string keyFindingId = "findingId";
    private const string keyRecommendation = "recommendation";
    private const string keyDomainId = "domainId";
    private const string keyStatus = "status";
    private const string keySeverity = "severity";
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
    [CosmosDBEnumString]
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
    public string FindingType
    {
        get => this.GetPropertyValue<string>(keyFindingType);
        set => this.SetPropertyValue(keyFindingType, value);
    }

    [EntityProperty(keyFindingSubType)]
    public string FindingSubType
    {
        get => this.GetPropertyValue<string>(keyFindingSubType);
        set => this.SetPropertyValue(keyFindingSubType, value);
    }

    [EntityProperty(keyFindingName)]
    public string FindingName
    {
        get => this.GetPropertyValue<string>(keyFindingName);
        set => this.SetPropertyValue(keyFindingName, value);
    }

    [EntityProperty(keyFindingId)]
    public string FindingId
    {
        get => this.GetPropertyValue<string>(keyFindingId);
        set => this.SetPropertyValue(keyFindingId, value);
    }

    [EntityProperty(keyRecommendation)]
    public string Recommendation
    {
        get => this.GetPropertyValue<string>(keyRecommendation);
        set => this.SetPropertyValue(keyRecommendation, value);
    }

    [EntityProperty(keyDomainId)]
    public string DomainId
    {
        get => this.GetPropertyValue<string>(keyDomainId);
        set => this.SetPropertyValue(keyDomainId, value);
    }

    [EntityProperty(keyStatus)]
    [CosmosDBEnumString]
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
    [CosmosDBEnumString]
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
    [CosmosDBEnumString]
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
    public string TargetEntityId
    {
        get => this.GetPropertyValue<string>(keyTargetEntityId);
        set => this.SetPropertyValue(keyTargetEntityId, value);
    }

    [EntityProperty(keyAssignedTo)]
    public string[] AssignedTo
    {
        get => this.GetPropertyValue<string[]>(keyAssignedTo);
        set => this.SetPropertyValue(keyAssignedTo, value);
    }

    private ActionSystemDataWrapper? systemData;

    [EntityProperty(keySystemData, true)]
    public ActionSystemDataWrapper SystemData
    {
        get => this.systemData ??= this.GetPropertyValueAsWrapper<ActionSystemDataWrapper>(keySystemData);
        set
        {
            this.SetPropertyValueFromWrapper(keySystemData, value);
            this.systemData = null;
        }
    }
}
