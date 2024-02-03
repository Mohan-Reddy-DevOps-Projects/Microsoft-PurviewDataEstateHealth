namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[CosmosDBContainer("DHControlStatusPalette")]
public class DHControlStatusPaletteWrapper(JObject jObject) : ContainerEntityBaseWrapper(jObject)
{
    private const string keyName = "name";
    private const string keyColor = "color";

    public DHControlStatusPaletteWrapper() : this(new JObject()) { }

    [EntityProperty(keyName)]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyColor)]
    public string Color
    {
        get => this.GetPropertyValue<string>(keyColor);
        set => this.SetPropertyValue(keyColor, value);
    }
}
