// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;

public class ActionSystemDataWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    public const string keyCreatedAt = "createdAt";
    private const string keyCreatedBy = "createdBy";
    private const string keyHintCount = "hintCount";
    private const string keyLastModifiedAt = "lastModifiedAt";
    private const string keyLastModifiedBy = "lastModifiedBy";
    private const string keyLastHintAt = "lastHintAt";
    private const string keyResolvedAt = "resolvedAt";
    private const string keyResolvedBy = "resolvedBy";

    public ActionSystemDataWrapper() : this([]) { }

    public ActionSystemDataWrapper(DateTime createdAt, string createdBy) : this(new JObject())
    {
        this.CreatedAt = createdAt;
        this.LastModifiedAt = createdAt;
        this.CreatedBy = createdBy;
        this.LastModifiedBy = createdBy;
        this.HintCount = 1;
    }


    [EntityProperty(keyCreatedAt, true)]
    public DateTime? CreatedAt
    {
        get => this.GetPropertyValue<DateTime?>(keyCreatedAt);
        set => this.SetPropertyValue(keyCreatedAt, value);
    }

    [EntityProperty(keyCreatedBy, true)]
    public string? CreatedBy
    {
        get => this.GetPropertyValue<string>(keyCreatedBy);
        private set => this.SetOrRemovePropertyValue(keyCreatedBy, value);
    }

    [EntityProperty(keyLastModifiedAt, true)]
    public DateTime? LastModifiedAt
    {
        get => this.GetPropertyValue<DateTime?>(keyLastModifiedAt);
        set => this.SetPropertyValue(keyLastModifiedAt, value);
    }

    [EntityProperty(keyLastModifiedBy, true)]
    public string? LastModifiedBy
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

    [EntityProperty(keyResolvedAt, true)]
    public DateTime? ResolvedAt
    {
        get => this.GetPropertyValue<DateTime?>(keyResolvedAt);
        set => this.SetOrRemovePropertyValue(keyResolvedAt, value);
    }

    [EntityProperty(keyResolvedBy, true)]
    public string? ResolvedBy
    {
        get => this.GetPropertyValue<string>(keyResolvedBy);
        private set => this.SetOrRemovePropertyValue(keyResolvedBy, value);
    }

    public void OnModify(string? modifiedBy, DateTime? modifiedAt = null)
    {
        var now = DateTime.UtcNow;
        this.LastModifiedAt = modifiedAt ?? now;
        this.LastModifiedBy = modifiedBy;
    }

    public void OnHint(DateTime? hintAt = null)
    {
        var now = DateTime.UtcNow;
        this.LastHintAt = hintAt ?? now;
        this.HintCount += 1;
    }
    public void OnResolve(string? resolvedBy, DateTime? resolvedAt = null)
    {
        var now = DateTime.UtcNow;
        this.ResolvedAt = resolvedAt ?? now;
        this.ResolvedBy = resolvedBy;
    }
    public void OnActive()
    {
        this.ResolvedAt = null;
        this.ResolvedBy = null;
    }
}
