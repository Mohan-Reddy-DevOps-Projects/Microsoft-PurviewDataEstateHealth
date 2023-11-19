// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

/// <summary>
/// Provides behavior on the data access layer level.
/// </summary>
public static class DataAccessLayer
{
    /// <summary>
    /// The default timeout for a request
    /// </summary>
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The default lifetime of a message handler
    /// </summary>
    private static readonly TimeSpan DefaultMessageHandlerLifetime = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan OverallTimeoutSeconds = TimeSpan.FromSeconds(100);

    private const string DefaultUserAgent = "DGHealth";

    /// <summary>
    /// Initializes the data access layer.
    /// </summary>
    /// <param name="services">Gives the data access layer a chance to configure its dependency injection.</param>
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddSingleton<AzureCredentialFactory>();
        services.AddExposureControl();
        services.AddMetadataServiceHttpClient(MetadataServiceClientFactory.HttpClientName);
        services.AddSingleton<MetadataServiceClientFactory>();
        services.AddSingleton<IMetadataAccessorService, MetadataAccessorService>();

        services.AddSingleton<ITableStorageClient<AccountStorageTableConfiguration>>(
        serviceProvider => new TableStorageClient<AccountStorageTableConfiguration>(
            serviceProvider.GetRequiredService<IOptions<AccountStorageTableConfiguration>>(),
            serviceProvider.GetRequiredService<AzureCredentialFactory>()));

        services.AddSingleton<ITableStorageClient<SparkPoolTableConfiguration>>(
            serviceProvider => new TableStorageClient<SparkPoolTableConfiguration>(
                serviceProvider.GetRequiredService<IOptions<SparkPoolTableConfiguration>>(),
                serviceProvider.GetRequiredService<AzureCredentialFactory>()));

        services.AddSingleton<IStorageAccountRepository<ProcessingStorageModel>, ProcessingStorageRepository>();
        services.AddSingleton<IServerlessPoolClient, ServerlessPoolClient>();
        services.AddSingleton<IServerlessQueryRequestBuilder, ServerlessQueryRequestBuilder>();
        services.AddSingleton<IServerlessQueryExecutor, ServerlessQueryExecutor>();
        services.AddSingleton<ISparkPoolRepository<SparkPoolModel>, SynapseSparkPoolRepository>();

        services.AddScoped<IDataEstateHealthSummaryRepository, DataEstateHealthSummaryRepository>();
        services.AddScoped<IHealthActionRepository, HealthActionRepository>();
        services.AddScoped<IHealthScoreRepository, HealthScoreRepository>();
        services.AddScoped<IBusinessDomainRepository, BusinessDomainRepository>();
        services.AddScoped<IHealthControlRepository, HealthControlRepository>();


        return services;
    }

    /// <summary>
    /// Exposure Control service.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddExposureControl(
        this IServiceCollection services)
    {
        services
            .AddSingleton<IExposureControlClient, ExposureControlClient>()
            .AddSingleton<IAccountExposureControlConfigProvider, AccountExposureControlConfigProvider>();

        return services;
    }

    /// <summary>
    /// Register the metadata service http client 
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">The user agent for the http client</param>
    /// <returns>Http client builder</returns>
    private static IHttpClientBuilder AddMetadataServiceHttpClient(this IServiceCollection services, string name)
    {
        HttpClientSettings httpClientSettings = new()
        {
            Name = name,
            UserAgent = DefaultUserAgent,
            RetryCount = 5
        };

        return services.AddCustomHttpClient<MetadataServiceConfiguration>(httpClientSettings,
            (serviceProvider, request, policy) => { });
    }

    /// <summary>
    /// Register the http client with the given settings
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <param name="services"></param>
    /// <param name="settings"></param>
    /// <param name="configureRetryPolicy"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddCustomHttpClient<TConfig>(
        this IServiceCollection services,
        HttpClientSettings settings,
        Action<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> configureRetryPolicy = null)
        where TConfig : BaseCertificateConfiguration
    {
        return services
            .AddHttpClient(settings.Name, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
                client.Timeout = settings.Timeout ?? DefaultRequestTimeout;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            })
            .SetHandlerLifetime(settings.HandlerLifetime ?? DefaultMessageHandlerLifetime)
            .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
            {
                ICertificateLoaderService certificateLoaderService = serviceProvider.GetRequiredService<ICertificateLoaderService>();
                IOptions<TConfig> configOptions = serviceProvider.GetRequiredService<IOptions<TConfig>>();
                var messageHandler = new CertificateHandler<TConfig>(certificateLoaderService, configOptions);

                IDataEstateHealthLogger logger = serviceProvider.GetRequiredService<IDataEstateHealthLogger>();
                logger.LogInformation($"Created a new {nameof(SocketsHttpHandler)} instance named '{settings.Name}' for outbound calls");

                return messageHandler;
            })
            .ConfigureHttpClient((client) => client.Timeout = settings.Timeout ?? OverallTimeoutSeconds)
            .AddPolicyHandler((serviceProvider, request) =>
            {
                if (configureRetryPolicy != null)
                {
                    IAsyncPolicy<HttpResponseMessage> policy = PollyRetryPolicies.GetHttpClientTransientRetryPolicy(
                        onRetry: LoggerRetryActionFactory.CreateHttpClientRetryAction(
                            serviceProvider.GetService<IDataEstateHealthRequestLogger>(),
                            settings.Name),
                        retryCount: settings.RetryCount);
                    
                    configureRetryPolicy(serviceProvider, request, policy);
                    return policy;
                }

                // Default or fallback policy
                return PollyRetryPolicies.GetHttpClientTransientRetryPolicy(
                    onRetry: LoggerRetryActionFactory.CreateHttpClientRetryAction(
                        serviceProvider.GetService<IDataEstateHealthRequestLogger>(),
                        settings.Name),
                    retryCount: settings.RetryCount);
            });
    }
}
