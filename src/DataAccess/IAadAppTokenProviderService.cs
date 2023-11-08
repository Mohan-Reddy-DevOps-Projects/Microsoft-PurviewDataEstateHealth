// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Identity.Client;

/// <summary>
/// The token service provider for 1P or 3P Aad application.
/// </summary>
public interface IAadAppTokenProviderService<TAadConfig>
{
    /// <summary>
    /// Get token for target resource.
    /// </summary>
    public Task<AuthenticationResult> GetTokenAsync(string targetResource, bool forceTokenRefresh = false);

    /// <summary>
    /// Get token with tenant and target resource.
    /// </summary>
    public Task<AuthenticationResult> GetTokenForTenantAsync(
       string tenantId,
       string targetResource,
       bool forceTokenRefresh = false);
}
