// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
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
    /// <param name="azureCredentials"></param>
    public static IServiceCollection AddCoreLayer(this IServiceCollection services, TokenCredential azureCredentials)
    {
        services.AddSingleton(azureCredentials);
        services.AddSingleton<IExceptionAdapterService, ExceptionAdapterService>();
        services.AddSingleton<ICertificateLoaderService, CertificateLoaderService>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IStorageCredentialsProvider, StorageCredentialsProvider>();
        services.AddSingleton<IKeyVaultAccessorService, KeyVaultAccessorService>();
        services.AddSingleton<IBlobStorageAccessor, BlobStorageAccessor>();
        services.AddSingleton<AadAppTokenProviderService<FirstPartyAadAppConfiguration>>();
        services.AddSingleton<IComponentContextFactory, ComponentContextFactory>();
        services.AddSingleton<ServiceHealthCheck>();

        services.AddPowerBI();
        services.AddServerlessPool();

        services.AddScoped<IRequestHeaderContext, RequestHeaderContext>();
        services.AddScoped<ICoreLayerFactory, CoreLayerFactory>();
       
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
        services.AddSingleton<AadAppTokenProviderService<PowerBIAuthConfiguration>>();
        services.AddSingleton<IPowerBIService, PowerBIService>();
        services.AddSingleton<PowerBIFactory>();

        return services;
    }

    /// <summary>
    /// Initializes the Synapse Serverless Pool services.
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddServerlessPool(this IServiceCollection services)
    {
        services.AddSingleton<AadAppTokenProviderService<ServerlessPoolAuthConfiguration>>();
        services.AddSingleton<IServerlessPoolClient, ServerlessPoolClient>();

        return services;
    }

    /// <summary>
    /// Add commands
    /// </summary>
    /// <param name="services">Service collection</param>
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<IDatasetCommand, DatasetCommand>();
        services.AddScoped<IProfileCommand, ProfileCommand>();
        services.AddScoped<IWorkspaceCommand, WorkspaceCommand>();
        return services;
    }
}
