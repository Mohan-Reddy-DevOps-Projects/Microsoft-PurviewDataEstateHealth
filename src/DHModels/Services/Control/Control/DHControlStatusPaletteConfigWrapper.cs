﻿namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHControlStatusPaletteConfigWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyTargetScore = "targetScore";
    private const string keyFallbackStatusPaletteId = "fallbackStatusPaletteId";
    private const string keyStatusPaletteRules = "statusPaletteRules";

    public DHControlStatusPaletteConfigWrapper() : this([]) { }

    [EntityProperty(keyTargetScore)]
    [EntityRequiredValidator]
    [EntityNumberValidator(Min = 0, Max = 1)]
    public double TargetScore
    {
        get => this.GetPropertyValue<double>(keyTargetScore);
        set => this.SetPropertyValue(keyTargetScore, value);
    }

    [EntityProperty(keyFallbackStatusPaletteId)]
    [EntityRequiredValidator]
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
