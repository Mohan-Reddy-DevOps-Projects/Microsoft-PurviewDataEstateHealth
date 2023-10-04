// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// ProfileModel model implementation class.
/// </summary>
public class ProfileModel : EntityModel, IProfileModel
{
    /// <summary>
    /// Instantiate instance of ProfileModel.
    /// </summary>
    public ProfileModel()
    {
    }

    /// <summary>
    /// The service principle id that created the profile
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// The PowerBI tenant which of this profile.
    /// </summary>
    public Guid TenantId { get; set; }
}
