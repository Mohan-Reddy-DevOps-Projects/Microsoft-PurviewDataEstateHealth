namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;

public class DHControlStatusPaletteWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHControlStatusPaletteWrapper>(jObject)
{
    private const string keyName = "name";
    private const string keyColor = "color";
    private const string keyReserved = "reserved";

    public static DHControlStatusPaletteWrapper Create(JObject jObject)
    {
        return new DHControlStatusPaletteWrapper(jObject);
    }

    public DHControlStatusPaletteWrapper() : this([]) { }

    [EntityRequiredValidator]
    [EntityNameValidator]
    [EntityProperty(keyName)]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityRequiredValidator]
    [EntityRegexValidator("^#([A-Fa-f0-9]{8}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    [EntityProperty(keyColor)]
    public string Color
    {
        get => this.GetPropertyValue<string>(keyColor);
        set => this.SetPropertyValue(keyColor, value);
    }

    [EntityProperty(keyReserved, true)]
    public bool Reserved
    {
        get => this.GetPropertyValue<bool>(keyReserved);
        set => this.SetPropertyValue(keyReserved, value);
    }
}
