// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using Microsoft.Extensions.DependencyInjection;
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
            services.AddDbContext<ControlDBContext>();
            services.AddScoped<DHControlRepository>();
            services.AddScoped<DHControlStatusPaletteRepository>();
            services.AddScoped<DHScoreRepository>();
            services.AddScoped<MQAssessmentRepository>();

            services.AddDbContext<ActionDBContext>();
            services.AddScoped<DataHealthActionRepository>();

            services.AddScheduleServiceHttpClient(ScheduleServiceClientFactory.HttpClientName);
            services.AddSingleton<ScheduleServiceClientFactory>();
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

            return services.AddDHCustomHttpClient<ScheduleConfiguration>(httpClientSettings,
                (serviceProvider, request, policy) => { });
        }
    }
}
