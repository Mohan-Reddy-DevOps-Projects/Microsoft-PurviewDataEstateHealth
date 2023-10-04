// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Text.Json;

/// <summary>
/// Profile request.
/// </summary>
public sealed class ProfileRequest : IProfileRequest
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ProfileRequest() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="other"></param>
    public ProfileRequest(IProfileRequest other)
    {
        this.AccountId = other.AccountId;
        this.ProfileId = other.ProfileId;
        this.ProfileName = other.ProfileName;
    }

    /// <inheritdoc/>
    public Guid AccountId { get; set; }

    /// <inheritdoc/>
    public Guid ProfileId { get; set; }

    /// <inheritdoc/>
    public string ProfileName { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
