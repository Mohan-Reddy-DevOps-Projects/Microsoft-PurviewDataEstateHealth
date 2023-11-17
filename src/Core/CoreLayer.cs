// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<IExceptionAdapterService, ExceptionAdapterService>();
        services.AddSingleton<ICertificateLoaderService, CertificateLoaderService>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IStorageCredentialsProvider, StorageCredentialsProvider>();
        services.AddSingleton<IKeyVaultAccessorService, KeyVaultAccessorService>();
        services.AddSingleton<IBlobStorageAccessor, BlobStorageAccessor>();
        services.AddSingleton<IAadAppTokenProviderService<FirstPartyAadAppConfiguration>,
            AadAppTokenProviderService<FirstPartyAadAppConfiguration>>();
        services.AddSingleton<IAadAppTokenProviderService<ServerlessPoolAuthConfiguration>,
            AadAppTokenProviderService<ServerlessPoolAuthConfiguration>>();
        services.AddSingleton<IComponentContextFactory, ComponentContextFactory>();
        services.AddSingleton<ServiceHealthCheck>();
        services.AddSingleton<IProcessingStorageManager, ProcessingStorageManager>();
        services.AddSingleton<ISynapseSparkExecutor, SynapseSparkExecutor>();
        services.AddSingleton<ISparkJobManager, SparkJobManager>();

        services.AddPowerBI();
        services.AddCommands();
        services.AddScoped<IPlatformSparkJobComponent, PlatformSparkJobComponent>();


        services.AddScoped<IRequestHeaderContext, RequestHeaderContext>();
        services.AddScoped<IRequestHeaderContextFactory, RequestHeaderContextFactory>();
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
        services.AddSingleton<IPowerBIService, PowerBIService>();
        services.AddSingleton<ICapacityAssignment, CapacityAssignment>();
        services.AddSingleton<PowerBIFactory>();

        return services;
    }

    /// <summary>
    /// Add commands
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<IPowerBICredentialComponent, PowerBICredentialComponent>();
        services.AddScoped<IDatasetCommand, DatasetCommand>();
        services.AddScoped<IReportCommand, ReportCommand>();
        services.AddScoped<IProfileCommand, ProfileCommand>();
        services.AddScoped<IWorkspaceCommand, WorkspaceCommand>();
        services.AddScoped<IDatabaseCommand, DatabaseCommand>();
        services.AddScoped<HealthProfileCommand>();
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
        services.AddScoped<IDeltaLakeOperatorFactory, DeltaLakeOperatorFactory>();

        return services;
    }
}
