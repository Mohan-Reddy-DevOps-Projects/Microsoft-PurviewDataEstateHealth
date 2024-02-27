// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Purview.DataGovernance.Reporting.Common;

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
            .Configure<AllowListedCertificateConfiguration>(configuration.GetSection("allowListedCertificate"))
            .AddProvisioningConfigurations(configuration);

        return services;
    }

    /// <summary>
    /// Configurations for worker service.
    /// </summary>
    public static IServiceCollection AddWorkerServiceConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCommonConfigurations(configuration)
            .AddEventHubConfigurations(configuration);

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
            .Configure<CertificateSetConfiguration>(configuration.GetSection("apiServiceCertificateSet"))
            .Configure<GenevaConfiguration>(configuration.GetSection("geneva"))
            .Configure<EnvironmentConfiguration>(configuration.GetSection("environment"))
            .Configure<JobConfiguration>(configuration.GetSection("jobManagerConfiguration"))
            .Configure<ServiceConfiguration>(configuration.GetSection("service"))
            .Configure<AccountStorageTableConfiguration>(configuration.GetSection("accountStorageTable"))
            .Configure<AuxStorageConfiguration>(configuration.GetSection("auxStorage"))
            .Configure<ProcessingStorageConfiguration>(configuration.GetSection("processingStorage"))
            .Configure<ProcessingStorageAuthConfiguration>(configuration.GetSection("processingStorageAuth"))
            .Configure<MetadataServiceConfiguration>(configuration.GetSection("metadataService"))
            .Configure<DataHealthApiServiceConfiguration>(configuration.GetSection("dataHealthApiService"))
            .Configure<ExposureControlConfiguration>(configuration.GetSection("exposureControl"))
            .Configure<ArtifactStoreServiceConfiguration>(configuration.GetSection("artifactStoreServiceConfiguration"))
            .Configure<PowerBIAuthConfiguration>(configuration.GetSection("powerBIAuth"))
            .Configure<ServerlessPoolAuthConfiguration>(configuration.GetSection("serverlessPoolAuth"))
            .Configure<ServerlessPoolConfiguration>(configuration.GetSection("serverlessPool"))
            .Configure<KeyVaultConfiguration>(configuration.GetSection("keyVault"))
            .Configure<SynapseSparkConfiguration>(configuration.GetSection("synapseSpark"))
            .Configure<SparkPoolTableConfiguration>(configuration.GetSection("sparkPoolTable"));

        return services;
    }

    /// <summary>
    /// Configurations common for api and worker service.
    /// </summary>
    private static IServiceCollection AddProvisioningConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions()
            .Configure<PartnerConfiguration>(configuration.GetSection("partner"));
        return services;
    }

    /// <summary>
    /// Configurations common for event hub processor.
    /// </summary>
    private static IServiceCollection AddEventHubConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions()
            .Configure<DataQualityEventHubConfiguration>(configuration.GetSection("eventhubConfiguration"))
            .Configure<DataCatalogEventHubConfiguration>(configuration.GetSection("eventhubConfiguration"))
            .Configure<DataAccessEventHubConfiguration>(configuration.GetSection("eventhubConfiguration"));
        return services;
    }
}
