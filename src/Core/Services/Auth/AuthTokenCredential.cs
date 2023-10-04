// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Identity.Client;

/// <summary>
/// Certificate credential class that consistently refreshes auth token upon expiration.
/// Useful to prevent downtime upon certificate rotation.
/// </summary>
internal sealed class AuthTokenCredential<TAadConfig> : TokenCredential where TAadConfig : AadAppConfiguration
{
    private readonly AadAppTokenProviderService<TAadConfig> tokenProvider;
    private readonly string resource;

    /// <summary>
    /// Creates AuthTokenCredential class
    /// </summary>
    /// <param name="tokenProvider">tokenProvider</param>
    /// <param name="resource"></param>
    public AuthTokenCredential(AadAppTokenProviderService<TAadConfig> tokenProvider, string resource)
    {
        this.tokenProvider = tokenProvider;
        this.resource = resource;
    }

    /// <summary>
    /// Synchronously calls <see cref="GetTokenAsync(TokenRequestContext, CancellationToken)"/>
    /// </summary>
    /// <param name="requestContext">Request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return this.GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Method that keeps token for authenticating with storage up-to-date. Called automatically when accessing storage
    /// and calls caller-supplied callback to retrieve updated certificate.
    /// </summary>
    /// <param name="requestContext">Request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        AuthenticationResult authResult = await this.tokenProvider.GetTokenAsync(this.resource);

        return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
    }
}
