namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class DHScheduleCallbackPayload
{
    public const string DGScheduleServiceOperatorName = "DGScheduleService";
    public const string GenevaActionOperatorName = "GenevaAction";

    [JsonProperty("controlId")]
    public string? ControlId { get; set; }

    [JsonProperty("operator")]
    public string Operator { get; set; } = "Unknown";

    [JsonConverter(typeof(StringEnumConverter))]
    public DHScheduleCallbackTriggerType TriggerType { get; set; } = DHScheduleCallbackTriggerType.Schedule;
}

public enum DHScheduleCallbackTriggerType
{
    Manually,
    Schedule
}