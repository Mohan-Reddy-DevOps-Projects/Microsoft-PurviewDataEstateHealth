namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert.NotifyTarget;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHNotifyTargetWrapperDerivedTypes.Email, EntityCategory.NofityTarget)]
public class DHNofityTargetEmailWrapper(JObject jObject) : DHNotifyTargetBaseWrapper(jObject)
{
    private const string keyEmail = "email";

    public DHNofityTargetEmailWrapper() : this([]) { }

    [EntityProperty(keyEmail)]
    [EntityRequiredValidator]
    [EntityRegexValidator(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
    public string Email
    {
        get => this.GetPropertyValue<string>(keyEmail);
        set => this.SetPropertyValue(keyEmail, value);
    }
}
