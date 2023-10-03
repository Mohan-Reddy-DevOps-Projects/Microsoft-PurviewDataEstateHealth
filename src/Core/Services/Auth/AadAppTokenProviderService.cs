// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

/// <summary>
/// The token service provider for 1P or 3P Aad application.
/// </summary>
internal sealed class AadAppTokenProviderService<TAadConfig>
    where TAadConfig : AadAppConfiguration
{
    private readonly TAadConfig configuration;
    private readonly IDataEstateHealthLogger logger;
    private readonly IMemoryCache cache;
    private readonly ICertificateLoaderService certificateLoaderService;
    private readonly TimeSpan timeSpanToRenewTokenBeforeExpiry;

    public AadAppTokenProviderService(
        IOptions<TAadConfig> configuration,
        ICertificateLoaderService certManager,
        IMemoryCache cache,
        IDataEstateHealthLogger logger)
    {
        this.configuration = configuration.Value;
        this.certificateLoaderService = certManager;
        this.cache = cache;
        this.logger = logger;
        this.timeSpanToRenewTokenBeforeExpiry = TimeSpan.FromMinutes(this.configuration.RenewBeforeExpiryInMinutes);
    }

    public async Task<AuthenticationResult> GetTokenAsync(
        string targetResource,
        bool forceTokenRefresh = false)
    {
        Uri authority = new(this.configuration.Authority);

        return await this.GetTokenInternalAsync(authority, targetResource, forceTokenRefresh);
    }

    public async Task<AuthenticationResult> GetTokenForTenantAsync(
        string tenantId,
        string targetResource,
        bool forceTokenRefresh = false)
    {
        Uri authority = this.configuration.GetAuthorityUri(tenantId);

        return await this.GetTokenInternalAsync(authority, targetResource, forceTokenRefresh);
    }

    private async Task<AuthenticationResult> GetTokenInternalAsync(
        Uri authority,
        string targetResource,
        bool forceTokenRefresh)
    {
        string cacheKey = (authority.ToString() + "_" + targetResource).ToLowerInvariant();
        this.logger.LogInformation($"AadAppTokenProviderService | GetTokenAsync for key: {cacheKey} with forceTokenRefresh: {forceTokenRefresh}.");

        if (forceTokenRefresh || !this.cache.TryGetValue(cacheKey, out AuthenticationResult cachedAuthenticationResult)
            || cachedAuthenticationResult.ExpiresOn <= DateTimeOffset.UtcNow.Add(this.timeSpanToRenewTokenBeforeExpiry))
        {
            X509Certificate2 certificate = await this.certificateLoaderService.LoadAsync(this.configuration.CertificateName, CancellationToken.None);

            string region = string.IsNullOrEmpty(this.configuration.AzureRegion) ? ConfidentialClientApplication.AttemptRegionDiscovery : this.configuration.AzureRegion;
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(this.configuration.ClientId)
                .WithAzureRegion(region)
                .WithLogging(this.LogCallback)
                .WithCertificate(certificate)
                .WithAuthority(authority)
            .Build();

            string[] scopes = new string[] { targetResource + "/.default" };

            AuthenticationResult newAuthenticationResult = await app.AcquireTokenForClient(scopes)
                .WithForceRefresh(forceTokenRefresh)
                .WithSendX5C(true) // required to enable SN+I authentication
                .ExecuteAsync();

            this.cache.Set(cacheKey, newAuthenticationResult);

            this.logger.LogInformation($"Using access token for key: {cacheKey} with expiresOn: {newAuthenticationResult.ExpiresOn}.");
            return newAuthenticationResult;
        }

        this.logger.LogInformation($"Using access token for key: {cacheKey} with expiresOn: {cachedAuthenticationResult.ExpiresOn}.");
        return cachedAuthenticationResult;
    }

    private void LogCallback(
        LogLevel level,
        string message,
        bool containsPii)
    {
        switch (level)
        {
            case LogLevel.Error:
                this.logger?.LogError(message, null);
                break;
            case LogLevel.Warning:
                this.logger?.LogError(message);
                break;
            default:
                // Do not log verbose or info
                break;
        }
    }
}
