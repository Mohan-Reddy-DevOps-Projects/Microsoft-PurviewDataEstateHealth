namespace Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System;

public class ContainerEntityAuditLogWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyTime = "time";
    private const string keyUser = "user";
    private const string keyAction = "action";

    public ContainerEntityAuditLogWrapper() : this([]) { }

    [EntityProperty(keyTime, true)]
    public DateTime Time
    {
        get => this.GetPropertyValue<DateTime>(keyTime);
        set => this.SetPropertyValue(keyTime, value);
    }

    [EntityProperty(keyUser, true)]
    public string User
    {
        get => this.GetPropertyValue<string>(keyUser);
        set => this.SetPropertyValue(keyUser, value);
    }

    [EntityProperty(keyAction, true)]
    public ContainerEntityAuditAction? Action
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyAction);
            return Enum.TryParse<ContainerEntityAuditAction>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetPropertyValue(keyAction, value?.ToString());
    }
}