namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHStorageConfigWrapperDerivedTypes.ADLSGen2, EntityCategory.StorageConfig)]
public class DHStorageConfigADLSGen2Wrapper(JObject jObject) : DHStorageConfigBaseWrapper(jObject)
{
    private const string keyStorageLocation = "storageLocation";
    private const string keyLocationURL = "locationURL";

    public DHStorageConfigADLSGen2Wrapper() : this([]) { }

    [EntityRequiredValidator]
    [EntityTypeProperty(keyStorageLocation)]
    public string StorageAccount
    {
        get => this.GetTypePropertyValue<string>(keyStorageLocation);
        set => this.SetTypePropertyValue(keyStorageLocation, value);
    }

    [EntityRequiredValidator]
    [EntityTypeProperty(keyLocationURL)]
    public string StorageDirectory
    {
        get => this.GetTypePropertyValue<string>(keyLocationURL);
        set => this.SetTypePropertyValue(keyLocationURL, value);
    }
}
