// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

/// <summary>
/// Defines the <see cref="AzureCredentialFactory" />.
/// </summary>
public sealed class AzureCredentialFactory
{
    private readonly EnvironmentConfiguration environmentConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureCredentialFactory"/> class.
    /// </summary>
    /// <param name="environmentConfig"></param>
    public AzureCredentialFactory(IOptions<EnvironmentConfiguration> environmentConfig)
    {
        this.environmentConfig = environmentConfig.Value;
    }

    /// <summary>
    /// Create DefaultAzureCredential
    /// </summary>
    /// <param name="authorityHost"></param>
    /// <returns></returns>
    public DefaultAzureCredential CreateDefaultAzureCredential(Uri authorityHost = null)
    {
        DefaultAzureCredentialOptions credentialOptions = new()
        {
            Retry =
            {
                Mode = RetryMode.Fixed,
                Delay = TimeSpan.FromSeconds(15),
                MaxRetries = 12,
                NetworkTimeout = TimeSpan.FromSeconds(100)
            },
            ExcludeManagedIdentityCredential = this.environmentConfig.IsDevelopmentEnvironment(),
            ExcludeWorkloadIdentityCredential = this.environmentConfig.IsDevelopmentEnvironment(),
            AuthorityHost = authorityHost
        };

        return new DefaultAzureCredential(credentialOptions);
    }
}
