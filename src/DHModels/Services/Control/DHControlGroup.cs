#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

using Newtonsoft.Json;

public class DHControlGroup : DHControlBase
{
    [JsonProperty("type")]
    public override DHControlType Type
    {
        get => DHControlType.Group;
        set { }
    }
}
