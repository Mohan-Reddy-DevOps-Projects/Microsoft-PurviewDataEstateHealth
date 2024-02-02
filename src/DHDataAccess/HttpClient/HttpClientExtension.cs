// <copyright file="HttpClientExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient
{
    using Microsoft.Azure.Purview.DataEstateHealth.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataGovernance.Reporting.Common;
    using Polly;
    using System;
    using System.Net.Http;

    public static class HttpClientExtension
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
        /// Register the http client with the given settings
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <param name="configureRetryPolicy"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddDHCustomHttpClient<TConfig>(
            this IServiceCollection services,
            HttpClientSettings settings,
            Action<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> configureRetryPolicy)
            where TConfig : BaseDHCertificateConfiguration
        {
            return services
                .AddHttpClient(settings.Name, client =>
                {
                    var userAgent = settings.UserAgent ?? DefaultUserAgent;
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                    client.Timeout = settings.Timeout ?? DefaultRequestTimeout;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                })
                .SetHandlerLifetime(settings.HandlerLifetime ?? DefaultMessageHandlerLifetime)
                .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
                {
                    var certificateLoaderService = serviceProvider.GetRequiredService<ICertificateLoaderService>();
                    var configOptions = serviceProvider.GetRequiredService<IOptions<TConfig>>();
                    var messageHandler = new CertificateHandler<TConfig>(certificateLoaderService, configOptions);

                    var logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
                    logger.LogInformation($"Created a new {nameof(SocketsHttpHandler)} instance named '{settings.Name}' for outbound calls");

                    return messageHandler;
                })
                .ConfigureHttpClient((client) => client.Timeout = settings.Timeout ?? OverallTimeoutSeconds)
                .AddPolicyHandler((serviceProvider, request) =>
                {
                    if (configureRetryPolicy != null)
                    {
                        var policy = PollyRetryPolicies.GetHttpClientTransientRetryPolicy(
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
}
