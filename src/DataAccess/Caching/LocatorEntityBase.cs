namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Locator entity base.
/// </summary>
public abstract class LocatorEntityBase
{
    /// <summary>
    /// The partition key.
    /// </summary>
    public Guid PartitionKey { get; }

    /// <summary>
    /// The name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorEntityBase"/> class.
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="name"></param>
    public LocatorEntityBase(Guid partitionKey, string name)
    {
        this.PartitionKey = partitionKey;
        this.Name = name;
    }

    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    /// <returns></returns>
    public abstract string ResourceId();

    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string ResourceId(string resourceId, string[] args) => ResourceIds.Create(resourceId, args);
}
