// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------


namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;

public class ActionSystemDataWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{

    private const string keyCreatedAt = "createdAt";
    private const string keyLastModifiedAt = "lastModifiedAt";
    private const string keyLastModifiedBy = "lastModifiedBy";
    private const string keyHintCount = "hintCount";
    private const string keyLastHintAt = "lastHintAt";


    public ActionSystemDataWrapper() : this(new JObject()) { }

    [EntityProperty(keyCreatedAt, true)]
    public DateTime? CreatedAt
    {
        get => this.GetPropertyValue<DateTime?>(keyCreatedAt);
        set => this.SetPropertyValue(keyCreatedAt, value);
    }

    [EntityProperty(keyLastModifiedAt, true)]
    public DateTime? LastModifiedAt
    {
        get => this.GetPropertyValue<DateTime?>(keyLastModifiedAt);
        set => this.SetPropertyValue(keyLastModifiedAt, value);
    }

    [EntityProperty(keyLastModifiedBy, true)]
    public string ExpiredBy
    {
        get => this.GetPropertyValue<string>(keyLastModifiedBy);
        private set => this.SetOrRemovePropertyValue(keyLastModifiedBy, value);
    }

    [EntityProperty(keyHintCount, true)]
    public int HintCount
    {
        get => this.GetPropertyValue<int>(keyHintCount);
        private set => this.SetOrRemovePropertyValue(keyHintCount, value);
    }

    [EntityProperty(keyLastHintAt, true)]
    public DateTime? LastHintAt
    {
        get => this.GetPropertyValue<DateTime?>(keyLastHintAt);
        set => this.SetPropertyValue(keyLastHintAt, value);
    }
}
