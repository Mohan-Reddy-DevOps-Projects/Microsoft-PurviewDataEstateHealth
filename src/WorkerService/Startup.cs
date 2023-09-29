// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

using System;
using System.IO;
using System.Text;
using global::Azure.Identity;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Purview.DataEstateHealth.Logger;

/// <summary>
/// Worker service startup.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Sets up configuration and DI services.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configurationManager">The configuration manager</param>
    public static void Configure(IServiceCollection services, ConfigurationManager configurationManager)
    {
        string appSettingsJson = Environment.GetEnvironmentVariable("APP_SETTINGS_JSON") ?? throw new Exception("environment variable 'APP_SETTINGS_JSON' is missing");
        configurationManager.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));

        services.AddWorkerServiceConfigurations(configurationManager);

        var tokenCredentials = new DefaultAzureCredential();
        services
            .AddLogger()
            .AddCoreLayer(tokenCredentials);

        services.AddHostedService<WorkerService>();
    }
}
