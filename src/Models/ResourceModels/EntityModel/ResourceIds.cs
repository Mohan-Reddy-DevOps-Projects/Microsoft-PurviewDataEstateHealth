namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Class to compute resourceId from resource
/// </summary>
public static class ResourceIds
{
    /// <summary>
    /// The processing storage
    /// </summary>
    public const string ProcessingStorage = "processingStorage/{0}";

    /// <summary>
    /// The PowerBI profile
    /// </summary>
    public const string Profile = $"profile";

    /// <summary>
    /// The spark pool
    /// </summary>
    public const string Spark = $"spark/{{0}}";

    /// <summary>
    /// The MDQ failed job
    /// </summary>
    public const string MDQFailedJob = $"mdqfailedjob/{{0}}";

    /// <summary>
    /// The background job definition
    /// </summary>
    public const string JobDefinition = $"jobDefinition/{{0}}";

    /// <summary>
    /// Create a formatted resource id.
    /// </summary>
    /// <param name="resourceType"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Create(string resourceType, string[] args)
    {
        return string.Format(resourceType, args);
    }
}
