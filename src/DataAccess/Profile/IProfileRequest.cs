// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// The profile request.
/// </summary>
public interface IProfileRequest
{
    /// <summary>
    /// The profile id.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// The profile name.
    /// </summary>
    string ProfileName { get; }

    /// <summary>
    /// The account id.
    /// </summary>
    Guid AccountId { get; }
}
