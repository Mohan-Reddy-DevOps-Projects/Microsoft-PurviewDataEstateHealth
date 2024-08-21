namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHStorageConfigWrapperDerivedTypes.ADLSGen2, EntityCategory.StorageConfig)]
public class DHStorageConfigADLSGen2Wrapper(JObject jObject) : DHStorageConfigBaseWrapper(jObject)
{
    private const string keyEndpoint = "endpoint";

    public DHStorageConfigADLSGen2Wrapper() : this([]) { }

    [EntityRequiredValidator]
    [EntityTypeProperty(keyEndpoint)]
    public string Endpoint
    {
        get => this.GetTypePropertyValue<string>(keyEndpoint);
        set => this.SetTypePropertyValue(keyEndpoint, value);
    }
}
