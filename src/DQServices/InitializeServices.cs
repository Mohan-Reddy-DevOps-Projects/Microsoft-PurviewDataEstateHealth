// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels
{
    using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services;

    public static class InitializeServices
    {
        public static void SetupDQServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions()
                .Configure<DataQualityServiceConfiguration>(configuration.GetSection(DataQualityServiceConfiguration.ConfigSectionName));

            HttpClientSettings httpClientSettings = new()
            {
                Name = DataQualityServiceClientFactory.HttpClientName,
                UserAgent = "DGHealth",
                RetryCount = 3
            };

            services.AddCustomHttpClient<DataQualityServiceConfiguration>(httpClientSettings,
                (serviceProvider, request, policy) => { });

            services.AddSingleton<DataQualityServiceClientFactory>();
            services.AddSingleton<IControlRepositoryFactory, ControlRepositoryFactory>();
            services.AddSingleton<IAssessmentRepositoryFactory, AssessmentRepositoryFactory>();
            services.AddSingleton<IDataQualityExecutionService, DataQualityExecutionService>();
        }
    }
}
