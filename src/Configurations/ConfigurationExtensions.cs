// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering configurations.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registers configurations common to the API and worker service.
    /// </summary>
    public static IServiceCollection AddCommonConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions()
            .Configure<EnvironmentConfiguration>(configuration.GetSection("environment"))
            .Configure<JobConfiguration>(configuration.GetSection("job"))
            .Configure<ServiceConfiguration>(configuration.GetSection("service"));

        return services;
    }
}
