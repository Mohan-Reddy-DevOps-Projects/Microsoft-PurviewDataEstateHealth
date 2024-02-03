namespace Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

public class ContainerEntityAuditLogWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyTime = "time";
    private const string keyUser = "user";
    private const string keyAction = "action";

    public ContainerEntityAuditLogWrapper() : this(new JObject()) { }

    [EntityProperty(keyTime, true)]
    [EntityRequiredValidator]
    public DateTime Timestamp
    {
        get => this.GetPropertyValue<DateTime>(keyTime);
        set => this.SetPropertyValue(keyTime, value);
    }

    [EntityProperty(keyUser, true)]
    [EntityRequiredValidator]
    public string User
    {
        get => this.GetPropertyValue<string>(keyUser);
        set => this.SetPropertyValue(keyUser, value);
    }

    [EntityProperty(keyAction, true)]
    [EntityRequiredValidator]
    [CosmosDBEnumString]
    public ContainerEntityAuditAction Action
    {
        get => this.GetPropertyValue<ContainerEntityAuditAction>(keyAction);
        set => this.SetPropertyValue(keyAction, value);
    }
}