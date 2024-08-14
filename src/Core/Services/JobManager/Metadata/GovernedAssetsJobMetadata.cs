namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.Metadata;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Newtonsoft.Json;

internal class GovernedAssetsJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// List accounts stage processed.
    /// </summary>
    [JsonProperty]
    public bool ListAccountsStageProcessed { get; set; }

    /// <summary>
    /// Spark pool id.
    /// </summary>
    [JsonProperty]
    public List<GovernedAssetsJobAccount> GovernedAssetsJobAccounts { get; set; } = new List<GovernedAssetsJobAccount>();

    /// <summary>
    /// Spark pool id.
    /// </summary>
    [JsonProperty]
    public string SparkPoolId { get; set; }

    /// <summary>
    /// Current schedule start time. 
    /// </summary>
    [JsonProperty]
    public DateTime? CurrentScheduleStartTime { get; set; }
}

internal class GovernedAssetsJobAccount
{
    /// <summary>
    /// Purview account model.
    /// </summary>
    [JsonProperty]
    public AccountServiceModel AccountServiceModel { get; set; }

    /// <summary>
    /// Billing Spark job id.
    /// </summary>
    [JsonProperty]
    public string ComputeGovernedAssetsSparkJobBatchId { get; set; }

    /// <summary>
    /// Catalog Spark job status.
    /// </summary>
    [JsonProperty]
    public DataPlaneSparkJobStatus ComputeGovernedAssetsSparkJobStatus { get; set; }
}
