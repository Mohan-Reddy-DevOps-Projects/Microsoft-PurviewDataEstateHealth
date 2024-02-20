namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;
using System.Collections.Generic;

public interface IContainerEntityWrapper
{
    public string Id { get; set; }

    public string? TenantId { get; set; }

    public string? AccountId { get; set; }

    public IEnumerable<ContainerEntityAuditLogWrapper> AuditLogs { get; set; }
}
