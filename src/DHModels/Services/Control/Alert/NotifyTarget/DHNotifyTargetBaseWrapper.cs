namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert.NotifyTarget;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json.Linq;

[EntityWrapper(EntityCategory.NofityTarget)]
public abstract class DHNotifyTargetBaseWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    public static DHNotifyTargetBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHNotifyTargetBaseWrapper>(EntityCategory.NofityTarget, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }
}

public static class DHNotifyTargetWrapperDerivedTypes
{
    public const string Email = "Email";
    public const string Role = "Role";
    public const string EntraId = "EntraID";
}

