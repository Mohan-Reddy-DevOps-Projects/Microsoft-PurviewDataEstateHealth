namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json.Linq;

public class DHStorageConfigBaseWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHStorageConfigBaseWrapper>(jObject)
{
    public static DHStorageConfigBaseWrapper Create(JObject jObject)
    {
        var entity = EntityWrapperHelper.CreateEntityWrapper<DHStorageConfigBaseWrapper>(EntityCategory.StorageConfig, EntityWrapperHelper.GetEntityType(jObject), jObject);
        return entity;
    }

    public DHStorageConfigBaseWrapper() : this([]) { }
}

public static class DHStorageConfigWrapperDerivedTypes
{
    public const string ADLSGen2 = "ADLSGen2";
}
