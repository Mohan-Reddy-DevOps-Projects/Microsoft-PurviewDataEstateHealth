namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHControlGlobalSchedulePayloadWrapper(JObject jObject) : DHControlScheduleWrapper(jObject)
{
    private const string keyAuditLogs = "auditLogs";

    private IEnumerable<ContainerEntityAuditLogWrapper>? auditLogs;

    [EntityProperty(keyAuditLogs, true)]
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
