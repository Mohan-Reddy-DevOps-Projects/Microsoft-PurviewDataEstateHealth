namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ContainerEntityBaseWrapper<T>(JObject jObject) : BaseEntityWrapper(jObject), IContainerEntityWrapper where T : ContainerEntityBaseWrapper<T>
{
    private const string keyId = "id";
    private const string keyAuditLogs = "auditLogs";

    public ContainerEntityBaseWrapper() : this([]) { }

    [JsonProperty(keyId)] // for cosmos DB
    [EntityProperty(keyId, true)]
    [EntityIdValidator]
    public string Id
    {
        get => this.GetPropertyValue<string>(keyId);
        set => this.SetPropertyValue(keyId, value);
    }

    public string? TenantId { get; set; }

    public string? AccountId { get; set; }

    private IEnumerable<ContainerEntityAuditLogWrapper>? auditLogs;

    [EntityProperty(keyAuditLogs)]
    public IEnumerable<ContainerEntityAuditLogWrapper> AuditLogs
    {
        get => this.auditLogs ??= this.GetPropertyValueAsWrappers<ContainerEntityAuditLogWrapper>(keyAuditLogs);
        set
        {
            this.SetPropertyValueFromWrappers(keyAuditLogs, value);
            this.auditLogs = value;
        }
    }

    public virtual void OnCreate(string userId)
    {
        this.Id = Guid.NewGuid().ToString();

        this.AuditLogs =
        [
            new()
            {
                Time = DateTime.UtcNow,
                User = userId,
                Action = ContainerEntityAuditAction.Create,
            },
        ];
    }

    public virtual void OnUpdate(T existWrapper, string userId)
    {
        this.Id = existWrapper.Id;

        var log = new ContainerEntityAuditLogWrapper()
        {
            Time = DateTime.UtcNow,
            User = userId,
            Action = ContainerEntityAuditAction.Update,
        };

        this.AuditLogs = (existWrapper.AuditLogs ?? []).Append(log);
    }
}

