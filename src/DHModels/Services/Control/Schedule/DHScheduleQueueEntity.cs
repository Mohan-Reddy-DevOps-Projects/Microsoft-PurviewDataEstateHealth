namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;

public class DHScheduleQueueEntity
{
    public const string DGScheduleServiceOperatorName = "DGScheduleService";
    public const string GenevaActionOperatorName = "GenevaAction";

    [JsonProperty("tenantId")]
    public required string TenantId { get; set; }

    [JsonProperty("accountId")]
    public required string AccountId { get; set; }

    [JsonProperty("controlId")]
    public string? ControlId { get; set; }

    [JsonProperty("operator")]
    public required string Operator { get; set; }

    [JsonProperty("triggerType")]
    public required string TriggerType { get; set; }

    [JsonProperty("tryCount")]
    public int TryCount { get; set; } = 0;
}
