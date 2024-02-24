// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using System;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Collections.Generic;
using Environment = Microsoft.Purview.DataGovernance.Common.Environment;

/// <summary>
/// Environment Configurations
/// </summary>
public class EnvironmentConfiguration
{
    private static readonly AzureEnvironment LocalhostAzureEnvironment = new AzureEnvironment
    {
        Name = "Localhost",
        AuthenticationEndpoint = "https://login.windows-ppe.net/",
        ResourceManagerEndpoint = "https://localhost/",
        ManagementEndpoint = "https://management.core.windows.net/",
        GraphEndpoint = "https://graph.microsoft-ppe.com/",
        StorageEndpointSuffix = "core.windows.net",
        KeyVaultSuffix = "vault-int.azure-int.net"
    };

    private static readonly AzureEnvironment AzureDogfoodCloudEnvironment = new AzureEnvironment
    {
        Name = "AzureDogfoodCloud",
        AuthenticationEndpoint = "https://login.windows-ppe.net/",
        ResourceManagerEndpoint = "https://api-dogfood.resources.windows-int.net/",
        ManagementEndpoint = "https://management.core.windows.net/",
        GraphEndpoint = "https://graph.microsoft-ppe.com/",
        StorageEndpointSuffix = "core.windows.net",
        KeyVaultSuffix = "vault-int.azure-int.net"
    };

    private static readonly AzureEnvironment AzureCanaryCloudEnvironment = new AzureEnvironment
    {
        Name = "AzureCanaryCloud",
        AuthenticationEndpoint = "https://login.microsoftonline.com/",
        ResourceManagerEndpoint = "https://management.azure.com/",
        ManagementEndpoint = "https://management.core.windows.net/",
        GraphEndpoint = "https://graph.microsoft.com/",
        StorageEndpointSuffix = "core.windows.net",
        KeyVaultSuffix = "vault.azure.net"
    };

    private static readonly AzureEnvironment AzureGlobalCloudEnvironment = new AzureEnvironment
    {
        Name = "AzureGlobalCloud",
        AuthenticationEndpoint = "https://login.microsoftonline.com/",
        ResourceManagerEndpoint = "https://management.azure.com/",
        ManagementEndpoint = "https://management.core.windows.net/",
        GraphEndpoint = "https://graph.microsoft.com/",
        StorageEndpointSuffix = "core.windows.net",
        KeyVaultSuffix = "vault.azure.net"
    };

    /// <summary>
    /// The current service environment
    /// </summary>
    public Environment Environment { get; set; }

    /// <summary>
    /// The current service location
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// The API Versions permitted in the environment.
    /// </summary>
    public List<string> PermittedApiVersions { get; set; }

    /// <summary>
    /// The AzureEnvironment for the current Environment
    /// </summary>
    public AzureEnvironment AzureEnvironment
    {
        get
        {
            switch (this.Environment)
            {
                case Environment.Development:
                    return EnvironmentConfiguration.LocalhostAzureEnvironment;
                case Environment.Dogfood:
                    return EnvironmentConfiguration.AzureDogfoodCloudEnvironment;
                case Environment.Canary:
                    return EnvironmentConfiguration.AzureCanaryCloudEnvironment;
                case Environment.Ci:
                case Environment.Dev:
                case Environment.Int:
                case Environment.Perf:
                case Environment.Prod:
                    return EnvironmentConfiguration.AzureGlobalCloudEnvironment;
                default:
                    return new AzureEnvironment();
            }
        }
    }

    /// <summary>
    /// Resource management endpoint Uri
    /// </summary>
    public Uri ResourceManagerEndpoint => new(this.AzureEnvironment.ResourceManagerEndpoint);

    /// <summary>
    /// Data plane uri for the given account
    /// </summary>
    /// <param name="accountName"></param>
    /// <returns></returns>
    public string GetDataPlaneUri(string accountName)
    {
        return FormattableString.Invariant($"{accountName}.{this.DataPlaneEndpoint}/share");
    }

    /// <summary>
    /// Graph endpoint uri
    /// </summary>
    public Uri GraphEndpoint => new(this.AzureEnvironment.GraphEndpoint);

    /// <summary>
    /// Boolean indicating whether the current environment is production or not
    /// </summary>
    /// <returns></returns>
    public bool IsProductionEnvironment()
    {
        return this.Environment == Environment.Prod || this.Environment == Environment.Canary;
    }

    /// <summary>
    /// Boolean indicating whether the current environment is production excluding canary
    /// </summary>
    /// <returns></returns>
    public bool IsProductionEnvironmentExcludingCanary()
    {
        return this.Environment == Environment.Prod;
    }

    /// <summary>
    /// Boolean indicating whether the current environment is development or not
    /// </summary>
    /// <returns></returns>
    public bool IsDevelopmentEnvironment()
    {
        return this.Environment == Environment.Development;
    }

    /// <summary>
    /// Boolean indicating whether the current environment is either development of dogfood
    /// </summary>
    /// <returns></returns>
    public bool IsDevelopmentOrDogfoodEnvironment()
    {
        return this.Environment == Environment.Development || this.Environment == Environment.Dogfood;
    }

    /// <summary>
    /// Authority Uri for the given tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public string GetAuthorityUri(Guid tenantId)
    {
        return FormattableString.Invariant(
            $"{this.AzureEnvironment.AuthenticationEndpoint}{tenantId}/oauth2/token");
    }

    /// <summary>
    /// Data plane endpoint
    /// </summary>
    private string DataPlaneEndpoint
    {
        get
        {
            switch (this.Environment)
            {
                case Environment.Development:
                case Environment.Dogfood:
                    return "purview.azure-test.com";
                case Environment.Canary:
                case Environment.Ci:
                case Environment.Dev:
                case Environment.Int:
                case Environment.Perf:
                case Environment.Prod:
                    return "purview.azure.com";
                default:
                    return "purview.azure.com";
            }
        }
    }

    /// <summary>
    /// Get the authority URI for a given domain.
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public string GetAuthorityUriFromDomain(string domain)
    {
        return FormattableString.Invariant(
            $"{this.AzureEnvironment.AuthenticationEndpoint}{domain}/.well-known/openid-configuration");
    }
}
