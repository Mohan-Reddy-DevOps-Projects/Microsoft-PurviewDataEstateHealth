// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using Microsoft.WindowsAzure.Storage.Auth;
using CoreTokenCredential = global::Azure.Core.TokenCredential;
using StorageTokenCredential = WindowsAzure.Storage.Auth.TokenCredential;

/// <inheritdoc/>
public class StorageCredentialsProvider : IStorageCredentialsProvider
{
    private const int RefreshBufferSeconds = 600;

    private const string StorageScope = "https://storage.azure.com/";

    private readonly CoreTokenCredential credential;

    /// <summary>
    /// Initializes a new instance of <see cref="StorageCredentialsProvider" /> class.
    /// </summary>
    /// <param name="credential"></param>
    public StorageCredentialsProvider(
        CoreTokenCredential credential)
    {
        this.credential = credential;
    }

    /// <inheritdoc/>
    public async Task<StorageCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        AccessToken token = await this.GetTokenAsync(cancellationToken);

        return new StorageCredentials(new StorageTokenCredential(token.Token));
    }

    /// <inheritdoc/>
    public async Task<StorageCredentials> GetRenewableCredentialsAsync(CancellationToken cancellationToken = default)
    {
        AccessToken initialToken = await this.GetTokenAsync(cancellationToken);
        TimeSpan refreshInterval = CalculateRefreshInterval(initialToken);

        return new StorageCredentials(
            new StorageTokenCredential(
                initialToken: initialToken.Token,
                periodicTokenRenewer: this.NewTokenAndFrequencyAsync,
                state: new object(),
                renewFrequency: refreshInterval));
    }

    private async Task<NewTokenAndFrequency> NewTokenAndFrequencyAsync(object state, CancellationToken cancellationToken)
    {
        AccessToken token = await this.GetTokenAsync(cancellationToken);
        TimeSpan refreshInterval = CalculateRefreshInterval(token);

        return new NewTokenAndFrequency(token.Token, refreshInterval);
    }

    private async Task<AccessToken> GetTokenAsync(CancellationToken cancellationToken)
    {
        return await this.credential.GetTokenAsync(
            new TokenRequestContext(new string[] { StorageScope }), cancellationToken);
    }

    private static TimeSpan CalculateRefreshInterval(AccessToken initialToken)
    {
        return initialToken.ExpiresOn
            .ToUniversalTime()
            .Subtract(DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(RefreshBufferSeconds)));
    }
}
