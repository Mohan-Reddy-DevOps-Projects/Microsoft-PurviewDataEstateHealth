// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------


namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;


[JsonConverter(typeof(StringEnumConverter))]
public enum DHActionType
{
    AssessmentAction,
    ProfilingAction
}

public class ActionExtraPropertiesWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{

    private const string keyType = "type";
    private const string KeyData = "data";

    public ActionExtraPropertiesWrapper() : this([]) { }

    [EntityProperty(keyType)]
    public DHActionType Type
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyType);
            return Enum.TryParse<DHActionType>(enumStr, true, out var result) ? result : DHActionType.AssessmentAction;
        }
        set => this.SetPropertyValue(keyType, value.ToString());
    }

    [EntityProperty(KeyData)]
    public JObject Data
    {
        get => this.GetPropertyValue<JObject>(KeyData);
        set => this.SetPropertyValue(KeyData, value);
    }

}
