// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding configurations
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Configurations for api service.
    /// </summary>
    public static IServiceCollection AddApiServiceConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCommonConfigurations(configuration)
            .Configure<SslConfiguration>(configuration.GetSection("ssl"))
            .Configure<AllowListedCertificateConfiguration>(configuration.GetSection("allowListedCertificate"))
            .Configure<CertificateSetConfiguration>(configuration.GetSection("apiServiceCertificateSet"))
            .Configure<PowerBIAuthConfiguration>(configuration.GetSection("powerBIAuth"))
            .Configure<ServerlessPoolAuthConfiguration>(configuration.GetSection("serverlessPoolAuth"))
            .Configure<ServerlessPoolConfiguration>(configuration.GetSection("serverlessPool"))
            .Configure<KeyVaultConfiguration>(configuration.GetSection("keyVault"))
            .Configure<AuxStorageConfiguration>(configuration.GetSection("auxStorage"))
            .Configure<FirstPartyAadAppConfiguration>(configuration.GetSection("firstPartyAadApp"));

        return services;
    }

    /// <summary>
    /// Configurations for worker service.
    /// </summary>
    public static IServiceCollection AddWorkerServiceConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCommonConfigurations(configuration);

        return services;
    }

    /// <summary>
    /// Configurations common for api and worker service.
    /// </summary>
    private static IServiceCollection AddCommonConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions()
            .Configure<GenevaConfiguration>(configuration.GetSection("geneva"))
            .Configure<EnvironmentConfiguration>(configuration.GetSection("environment"))
            .Configure<JobConfiguration>(configuration.GetSection("job"))
            .Configure<ServiceConfiguration>(configuration.GetSection("service"));
        return services;
    }
}
