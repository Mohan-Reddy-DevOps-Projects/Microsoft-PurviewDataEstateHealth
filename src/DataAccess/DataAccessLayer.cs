// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.MetadataStore;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

/// <summary>
/// Provides behavior on the data access layer level.
/// </summary>
public static class DataAccessLayer
{
    private const int OverallTimeoutSeconds = 160;

    private const int PerRequestTimeoutSeconds = 30;

    private static readonly TimeSpan DefaultMessageHandlerLifetime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Initializes the data access layer.
    /// </summary>
    /// <param name="services">Gives the data access layer a chance to configure its dependency injection.</param>
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {
        _ = services.AddHttpClient<IProjectBabylonMetadataClient, ProjectBabylonMetadataClient>((httpClient, _)
            => new ProjectBabylonMetadataClient(httpClient, true))
            .ConfigurePrimaryHttpMessageHandler((serviceProvider) => new MetadataCertificateHandler(
                    serviceProvider.GetRequiredService<ICertificateLoaderService>(),
                    serviceProvider.GetRequiredService<IOptions<MetadataServiceConfiguration>>()))
            .SetHandlerLifetime(DefaultMessageHandlerLifetime)
            .ConfigureHttpClient((client) => client.Timeout = TimeSpan.FromSeconds(OverallTimeoutSeconds))
            .AddPolicyHandler((serviceProvider, _) => PollyRetryPolicies.GetHttpClientTransientRetryPolicy(
                    onRetry: LoggerRetryActionFactory.CreateHttpClientRetryAction(
                        serviceProvider.GetService<IDataEstateHealthRequestLogger>(),
                        nameof(MetadataAccessorService)),
                    retryCount: 5))

            .AddPolicyHandler(Policy.TimeoutAsync(PerRequestTimeoutSeconds).AsAsyncPolicy<HttpResponseMessage>());

        _ = services.AddScoped<IMetadataAccessorService, MetadataAccessorService>();
        _ = services.AddScoped<IDataEstateHealthSummaryRepository, DataEstateHealthSummaryRepository>();
        _ = services.AddScoped<IHealthActionRepository, HealthActionRepository>();
        _ = services.AddScoped<IHealthScoreRepository, HealthScoreRepository>();
        _ = services.AddScoped<IBusinessDomainRepository, BusinessDomainRepository>();

        return services;
    }
}
