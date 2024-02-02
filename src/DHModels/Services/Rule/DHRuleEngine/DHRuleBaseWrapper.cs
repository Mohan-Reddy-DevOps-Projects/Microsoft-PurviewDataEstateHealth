#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

public abstract class DHRuleBaseWrapper(JObject jObject) : DHRuleOrGroupBaseWrapper(jObject)
{
    private const string keyCheckPoint = "checkPoint";

    [EntityProperty(keyCheckPoint)]
    [CosmosDBEnumString]
    public DHCheckPoint CheckPoint
    {
        get => this.GetPropertyValue<DHCheckPoint>(keyCheckPoint);
        set => this.SetPropertyValue(keyCheckPoint, value);
    }
}
