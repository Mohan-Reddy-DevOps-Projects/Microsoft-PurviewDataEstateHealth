// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

/// <summary>
/// Partner details.
/// </summary>
public interface IPartnerDetails
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the endpoint.
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    /// Gets a value indicating whether [validate response].
    /// </summary>
    bool ValidateResponse { get; }

    /// <summary>
    /// Gets the create or update timeout seconds.
    /// </summary>
    int CreateOrUpdateTimeoutSeconds { get; }

    /// <summary>
    /// Gets the delete timeout seconds.
    /// </summary>
    int DeleteTimeoutSeconds { get; }

    /// <summary>
    /// Gets the partner dependencies.
    /// </summary>
    string[] DependsOn { get; }
}
