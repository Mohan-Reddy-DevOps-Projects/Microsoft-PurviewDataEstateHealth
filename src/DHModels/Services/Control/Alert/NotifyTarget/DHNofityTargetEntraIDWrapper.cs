namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert.NotifyTarget;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHNotifyTargetWrapperDerivedTypes.EntraId, EntityCategory.NofityTarget)]
public class DHNofityTargetEntraIDWrapper(JObject jObject) : DHNotifyTargetBaseWrapper(jObject)
{
    private const string keyObjectId = "objectId";

    public DHNofityTargetEntraIDWrapper() : this([]) { }

    [EntityProperty(keyObjectId)]
    [EntityRequiredValidator]
    public string ObjectId
    {
        get => this.GetPropertyValue<string>(keyObjectId);
        set => this.SetPropertyValue(keyObjectId, value);
    }
}
