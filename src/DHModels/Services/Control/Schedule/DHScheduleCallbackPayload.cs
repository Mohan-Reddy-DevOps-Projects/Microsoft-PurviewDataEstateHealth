namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleCallbackPayload
{
    [JsonProperty("tenantId")]
    public required string TenantId { get; set; }

    [JsonProperty("accountId")]
    public required string AccountId { get; set; }

    [JsonProperty("controlId")]
    public string? ControlId { get; set; }
}
