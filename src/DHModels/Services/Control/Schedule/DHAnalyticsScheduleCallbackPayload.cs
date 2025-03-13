namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;


public class DHAnalyticsScheduleCallbackPayload
{
    public const string DGScheduleServiceOperatorName = "DGAnalyticsScheduleService";
    public const string GenevaActionOperatorName = "GenevaAction";

    [JsonProperty("controlId")]
    public string? ControlId { get; set; }

    [JsonProperty("operator")]
    public string Operator { get; set; } = "Unknown";

    [JsonProperty("triggerType")]
    [JsonConverter(typeof(StringEnumConverter))]
    public DHAnalyticsScheduleCallbackTriggerType TriggerType { get; set; } = DHAnalyticsScheduleCallbackTriggerType.Schedule;
}

public enum DHAnalyticsScheduleCallbackTriggerType
{
    Manually,
    Schedule
}

