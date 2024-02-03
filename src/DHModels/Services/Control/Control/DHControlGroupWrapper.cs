namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DHControlBaseWrapperDerivedTypes.Group, EntityCategory.Control)]
public class DHControlGroupWrapper(JObject jObject) : DHControlBaseWrapper(jObject)
{
    public DHControlGroupWrapper() : this(new JObject()) { }
}
