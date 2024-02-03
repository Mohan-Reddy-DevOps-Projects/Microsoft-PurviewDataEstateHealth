namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHControlStatusPaletteConfigWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyFallbackStatusPaletteId = "fallbackStatusPaletteId";
    private const string keyStatusPaletteRules = "statusPaletteRules";

    public DHControlStatusPaletteConfigWrapper() : this(new JObject()) { }

    [EntityProperty(keyFallbackStatusPaletteId)]
    public string FallbackStatusPaletteId
    {
        get => this.GetPropertyValue<string>(keyFallbackStatusPaletteId);
        set => this.SetPropertyValue(keyFallbackStatusPaletteId, value);
    }

    private IEnumerable<DHControlStatusPaletteUnitWrapper>? statusPaletteRules;

    [EntityProperty(keyStatusPaletteRules)]
    public IEnumerable<DHControlStatusPaletteUnitWrapper> StatusPaletteRules
    {
        get => this.statusPaletteRules ??= this.GetPropertyValueAsWrappers<DHControlStatusPaletteUnitWrapper>(keyStatusPaletteRules);
        set
        {
            this.SetPropertyValueFromWrappers(keyStatusPaletteRules, value);
            this.statusPaletteRules = value;
        }
    }
}
