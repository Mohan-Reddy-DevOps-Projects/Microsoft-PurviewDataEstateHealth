namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;

/// <summary>
/// Spark pool job model.
/// </summary>
public class SparkPoolJobModel
{
    /// <summary>
    /// Gets or sets the resource identifier of spark pool.
    /// </summary>
    public string PoolResourceId { get; set; }

    /// <summary>
    /// Gets or sets the job identifier.
    /// </summary>
    public string JobId { get; set; }
}
