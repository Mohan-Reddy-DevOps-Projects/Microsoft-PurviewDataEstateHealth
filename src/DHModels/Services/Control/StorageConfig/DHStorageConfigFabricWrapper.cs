namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHStorageConfigWrapperDerivedTypes.Fabric, EntityCategory.StorageConfig)]
public class DHStorageConfigFabricWrapper(JObject jObject) : DHStorageConfigBaseWrapper(jObject)
{
    private const string keyLocationURL = "locationURL";

    public DHStorageConfigFabricWrapper() : this([]) { }

    [EntityRequiredValidator]
    [EntityTypeProperty(keyLocationURL)]
    public string LocationURL
    {
        get => this.GetTypePropertyValue<string>(keyLocationURL);
        set => this.SetTypePropertyValue(keyLocationURL, value);
    }
}
