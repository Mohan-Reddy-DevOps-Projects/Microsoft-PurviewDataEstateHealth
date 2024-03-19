// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;

    public static class InitializeServices
    {
        public static void SetupDHDataAccessServices(this IServiceCollection services)
        {
            services.AddSingleton<DHCosmosDBContextAzureCredentialManager>();

            services.AddScheduleServiceHttpClient(ScheduleServiceClientFactory.HttpClientName);
            services.AddSingleton<ScheduleServiceClientFactory>();

            services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                var credential = serviceProvider.GetRequiredService<DHCosmosDBContextAzureCredentialManager>().Credential;
                var logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();

                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var cosmosDbEndpoint = configuration["cosmosDb:accountEndpoint"];

                return new CosmosClient(cosmosDbEndpoint, credential, new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Direct,
                    Serializer = new CosmosWrapperSerializer(logger),
                    AllowBulkExecution = true
                });
            });

            services.AddScoped<DHControlRepository>();
            services.AddScoped<DHControlStatusPaletteRepository>();
            services.AddScoped<DHScoreRepository>();
            services.AddScoped<DHAssessmentRepository>();
            services.AddScoped<DHControlScheduleRepository>();
            services.AddScoped<DHActionRepository>();
            services.AddScoped<DHComputingJobRepository>();
        }

        /// <summary>
        /// Register the schedule service http client 
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="name">The user agent for the http client</param>
        /// <returns>Http client builder</returns>
        public static IHttpClientBuilder AddScheduleServiceHttpClient(this IServiceCollection services, string name)
        {
            HttpClientSettings httpClientSettings = new()
            {
                Name = name,
                RetryCount = 3
            };

            return services.AddDHCustomHttpClient<DHScheduleConfiguration>(httpClientSettings,
                (serviceProvider, request, policy) => { });
        }
    }
}
