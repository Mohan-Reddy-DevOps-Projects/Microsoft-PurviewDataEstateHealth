namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert.NotifyTarget;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(DHNotifyTargetWrapperDerivedTypes.Role, EntityCategory.NofityTarget)]
public class DHNofityTargetRoleWrapper(JObject jObject) : DHNotifyTargetBaseWrapper(jObject)
{
    private const string keyRole = "role";

    public DHNofityTargetRoleWrapper() : this([]) { }

    [EntityProperty(keyRole)]
    [EntityRequiredValidator]
    public DHNotifyTargetRole? Role
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyRole);
            return Enum.TryParse<DHNotifyTargetRole>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyRole, value?.ToString());
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DHNotifyTargetRole
{
    ControlOwner
}