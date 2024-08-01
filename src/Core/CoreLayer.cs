// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.Core;
using Microsoft.Purview.DataGovernance.Common;
using Microsoft.Purview.DataGovernance.DeltaWriter;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.Reporting.Services;

/// <summary>
/// Provides behavior on the core layer level.
/// </summary>
public static class CoreLayer
{
    /// <summary>
    /// Initializes the core layer.
    /// </summary>
    /// <param name="services">Gives the core layer a chance to configure its dependency injection.</param>
    public static IServiceCollection AddCoreLayer(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateLoaderService, CertificateLoaderService>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IStorageCredentialsProvider, StorageCredentialsProvider>();
        services.AddSingleton<IKeyVaultAccessorService, KeyVaultAccessorService>();
        services.AddSingleton<IComponentContextFactory, ComponentContextFactory>();
        services.AddSingleton<ServiceHealthCheck>();
        services.AddSingleton<ProcessingStorageManager>();
        services.AddSingleton<IProcessingStorageManager>(impl =>
        {
            IProcessingStorageManager repository = impl.GetRequiredService<ProcessingStorageManager>();
            ICacheManager cacheManager = impl.GetRequiredService<ICacheManager>();
            IDataEstateHealthRequestLogger logger = impl.GetRequiredService<IDataEstateHealthRequestLogger>();
            return new InMemoryProcessingStorageCache(repository, TimeSpan.FromHours(1), cacheManager, logger);
        });
        services.AddSingleton<ISynapseSparkExecutor, SynapseSparkExecutor>();
        services.AddSingleton<ISparkJobManager, SparkJobManager>();
        services.AddSingleton<ICommonFieldValidationService, CommonFieldValidationService>();
        services.AddSingleton<IDeltaTableEventProcessor, DeltaTableEventProcessor>();

        services.AddPowerBI();
        services.AddCommands();
        services.AddScoped<ICatalogSparkJobComponent, CatalogSparkJobComponent>();
        services.AddScoped<IDimensionModelSparkJobComponent, DimensionModelSparkJobComponent>();
        services.AddScoped<IFabricSparkJobComponent, FabricSparkJobComponent>();
        services.AddScoped<IDataQualitySparkJobComponent, DataQualitySparkJobComponent>();
        services.AddScoped<IDatasetsComponent, DatasetsComponent>();
        services.AddScoped<IRefreshComponent, RefreshComponent>();

        services.AddScoped<Models.IRequestHeaderContext, Models.RequestHeaderContext>();
        services.AddScoped<ICoreLayerFactory, CoreLayerFactory>();

        services.AddScoped<IJobManager, JobManager>();
        services.AddSingleton<IJobManagementStorageAccountBuilder, JobManagementStorageAccountBuilder>();

        services.AddTransient<IAzureResourceManagerFactory, AzureResourceManagerFactory>();

        services.AddHealthChecks().AddCheck<ServiceHealthCheck>("Ready");

        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Initializes the PowerBI services.
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddPowerBI(this IServiceCollection services)
    {
        services.AddSingleton<IAadAppTokenProviderService<PowerBIAuthConfiguration>,
            AadAppTokenProviderService<PowerBIAuthConfiguration>>();
        services.AddSingleton(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            var certificateLoader = provider.GetRequiredService<ICertificateLoaderService>();
            var exposureControlConfig = provider.GetRequiredService<IOptions<ExposureControlConfiguration>>();
            return new CapacityProvider(powerBIProvider, certificateLoader, exposureControlConfig.Value);

        });
        services.AddSingleton<IBlobStorageAccessor, BlobStorageAccessor>(provider =>
        {
            var auxStorage = provider.GetRequiredService<IOptions<AuxStorageConfiguration>>();
            var logger = provider.GetRequiredService<IDataEstateHealthRequestLogger>();
            var credentialFactory = provider.GetRequiredService<AzureCredentialFactory>();

            return new BlobStorageAccessor(logger, credentialFactory, auxStorage.Value);

        });
        services.AddSingleton(provider =>
        {
            var logger = provider.GetRequiredService<IDataEstateHealthRequestLogger>();
            var aadService = provider.GetRequiredService<IAadAppTokenProviderService<PowerBIAuthConfiguration>>();
            var authConfig = provider.GetRequiredService<IOptions<PowerBIAuthConfiguration>>();
            return new PowerBIProvider(logger, aadService, authConfig.Value);
        });
        services.AddScoped<IHealthPBIReportComponent, HealthPBIReportComponent>();

        return services;
    }

    /// <summary>
    /// Add commands
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<IPowerBICredentialComponent, PowerBICredentialComponent>();
        services.AddScoped(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            return new TokenProvider(powerBIProvider);
        });
        services.AddScoped(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            var datasetProvider = provider.GetRequiredService<DatasetProvider>();
            return new ReportProvider(powerBIProvider, datasetProvider);
        });
        services.AddScoped(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            var blobStorageAccessor = provider.GetRequiredService<IBlobStorageAccessor>();
            var auxStorage = provider.GetRequiredService<IOptions<AuxStorageConfiguration>>();
            return new DatasetProvider(powerBIProvider, blobStorageAccessor, auxStorage.Value);
        });
        services.AddScoped(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            return new ProfileProvider(powerBIProvider);
        });
        services.AddScoped(provider =>
        {
            var powerBIProvider = provider.GetRequiredService<PowerBIProvider>();
            return new WorkspaceProvider(powerBIProvider);
        });
        services.AddScoped<IDatabaseCommand, DatabaseCommand>();
        services.AddScoped<HealthProfileCommand>();
        services.AddScoped<IHealthProfileCommand>(impl =>
         {
             IHealthProfileCommand repository = impl.GetRequiredService<HealthProfileCommand>();
             ICacheManager cacheManager = impl.GetRequiredService<ICacheManager>();
             IDataEstateHealthRequestLogger logger = impl.GetRequiredService<IDataEstateHealthRequestLogger>();
             return new InMemoryProfileCache(repository, TimeSpan.FromHours(1), cacheManager, logger);
         });
        services.AddScoped<HealthWorkspaceCommand>();
        services.AddScoped<IDatabaseManagementService, DatabaseManagementService>();

        return services;
    }

    /// <summary>
    /// Initializes the partner event processor services.
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddPartnerEventsProcessor(this IServiceCollection services)
    {
        services.AddSingleton<IPartnerEventsProcessorFactory, PartnerEventsProcessorFactory>();
        services.AddSingleton<DeltaWriterConfiguration>(provider =>
        {
            IOptions<EnvironmentConfiguration> envConfig = provider.GetRequiredService<IOptions<EnvironmentConfiguration>>();
            return new DeltaWriterConfiguration
            {
                IsDevelopmentEnvironment = envConfig.Value.IsDevelopmentEnvironment(),
            };
        });
        services.AddScoped<IDeltaLakeOperatorFactory, DeltaLakeOperatorFactory>(provider =>
        {
            var logger = provider.GetRequiredService<IDataEstateHealthRequestLogger>();
            var deltaWriterConfiguration = provider.GetRequiredService<DeltaWriterConfiguration>();
            return new DeltaLakeOperatorFactory(deltaWriterConfiguration, logger);
        });

        return services;
    }
}
