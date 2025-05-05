// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.SynapseSqlClient;

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
            //TODO: Uncomment this line once Manoj is done testing in Canary.
            // .Configure<AllowListedCertificateConfiguration>(configuration.GetSection("allowListedCertificate"))
            .AddProvisioningConfigurations(configuration);

        //TODO:Hotfix to let Manoj test in Canary. This needs to be removed once testing is complete.
        var certConfig = new AllowListedCertificateConfiguration();
        configuration.GetSection("allowListedCertificate").Bind(certConfig);

        var environmentConfig = configuration.GetSection("environment").Get<EnvironmentConfiguration>();

        if (environmentConfig?.IsCanaryEnvironment() == true)
        {
            const string canaryCertName = "prod-cus-client.dgcatalog.purview-service.azure.com";

            certConfig.AllowListedDataPlaneSubjectNames ??= [];

            if (!certConfig.AllowListedDataPlaneSubjectNames.Contains(canaryCertName))
            {
                certConfig.AllowListedDataPlaneSubjectNames.Add(canaryCertName);
            }
        }

        services.Configure<AllowListedCertificateConfiguration>(options =>
        {
            foreach (var property in typeof(AllowListedCertificateConfiguration).GetProperties())
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                object value = property.GetValue(certConfig);
                property.SetValue(options, value);
            }
        });
        ///////////////////////////////////////////////////

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
            .Configure<SparkPoolTableConfiguration>(configuration.GetSection("sparkPoolTable"))
            .Configure<MDQFailedJobTableConfiguration>(configuration.GetSection("mdqFailedJobTable"))
            .Configure<JobDefinitionTableConfiguration>(configuration.GetSection("jobDefinitionTable"))
            .Configure<TriggeredScheduleQueueConfiguration>(configuration.GetSection("triggeredScheduleQueue"));

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
