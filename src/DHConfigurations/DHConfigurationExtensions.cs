// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHConfigurations
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for adding configurations
    /// </summary>
    public static class DHConfigurationExtensions
    {
        /// <summary>
        /// Configurations common for api and worker service.
        /// </summary>
        public static IServiceCollection AddDHConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions()
                .Configure<ScheduleConfiguration>(configuration.GetSection("scheduleService"));

            return services;
        }
    }
}