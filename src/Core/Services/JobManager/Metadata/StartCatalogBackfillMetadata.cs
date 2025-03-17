namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

public enum CatalogBackfillStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public sealed class StartCatalogBackfillMetadata : StagedWorkerJobMetadata
{
    [JsonProperty]
    public CatalogBackfillStatus BackfillStatus { get; set; }

    [JsonProperty]
    public DateTime? StartTime { get; set; }

    [JsonProperty]
    public DateTime? EndTime { get; set; }

    [JsonProperty]
    public int ItemsProcessed { get; set; }

    [JsonProperty]
    public string ErrorMessage { get; set; }

    [JsonProperty]
    public string BackfillScope { get; set; } = "Global";

    [JsonProperty]
    public List<string> AccountIds { get; set; } = [];

    [JsonProperty]
    public int BatchAmount { get; set; } = 10;

    [JsonProperty]
    public int BufferTimeInMinutes { get; set; } = 5;
}