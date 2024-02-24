namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Holds information needed to locate a profile in persistence.
/// </summary>
public class ProfileKey : LocatorEntityBase
{
    /// <summary>
    /// Initialize Profile key
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    public ProfileKey(Guid accountId) : base(accountId, OwnerNames.Health)
    {
    }

    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    /// <returns></returns>
    public override string ResourceId() => ResourceId(ResourceIds.Profile, [this.Name.ToString()]);
}
