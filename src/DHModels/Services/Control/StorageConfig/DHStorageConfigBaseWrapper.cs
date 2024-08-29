namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

public class DHStorageConfigBaseWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHStorageConfigBaseWrapper>(jObject)
{
    private const string keyStatus = "status";

    public static DHStorageConfigBaseWrapper Create(JObject jObject)
    {
        var entity = EntityWrapperHelper.CreateEntityWrapper<DHStorageConfigBaseWrapper>(EntityCategory.StorageConfig, EntityWrapperHelper.GetEntityType(jObject), jObject);
        return entity;
    }

    public DHStorageConfigBaseWrapper() : this([]) { }

    [EntityProperty(keyStatus)]
    [EntityRequiredValidator]
    public DHStorageConfigStatus Status
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyStatus);
            return Enum.TryParse<DHStorageConfigStatus>(enumStr, true, out var result) ? result : DHStorageConfigStatus.Enabled;
        }
        set => this.SetPropertyValue(keyStatus, value.ToString());
    }
}

public static class DHStorageConfigWrapperDerivedTypes
{
    public const string Fabric = "Fabric";
    public const string ADLSGen2 = "ADLSGen2";
}

public enum DHStorageConfigStatus
{
    Disabled,
    Enabled
}