namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public abstract class ContainerEntityDynamicWrapper(JObject jObject) : DynamicEntityWrapper(jObject), IContainerEntityWrapper
{
    private const string keyId = "id";
    private const string keyAuditLogs = "auditLogs";

    public ContainerEntityDynamicWrapper() : this([]) { }

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
}

