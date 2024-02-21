namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleCallbackPayload
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }

    [JsonProperty("tenantId")]
    public required string TenantId { get; set; }

    [JsonProperty("accountId")]
    public required string AccountId { get; set; }
}
