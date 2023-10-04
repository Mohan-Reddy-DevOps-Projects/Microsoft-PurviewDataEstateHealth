// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Profile model
/// </summary>
public interface IProfileModel : IEntityModel
{
    /// <summary>
    /// The service principle id that created the profile
    /// </summary>
    Guid ClientId { get; }

    /// <summary>
    /// The PowerBI tenant which of this profile.
    /// </summary>
    Guid TenantId { get; }
}
