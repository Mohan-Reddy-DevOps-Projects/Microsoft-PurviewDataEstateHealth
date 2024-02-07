namespace Microsoft.Purview.DataEstateHealth.DHModels.Common.AuditLog;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum ContainerEntityAuditAction
{
    Create,
    Update,
}
