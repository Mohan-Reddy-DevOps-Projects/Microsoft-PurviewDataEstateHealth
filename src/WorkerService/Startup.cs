// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

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
        string appSettingsJson = System.Environment.GetEnvironmentVariable("APP_SETTINGS_JSON") ?? throw new Exception("environment variable 'APP_SETTINGS_JSON' is missing");
        configurationManager.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));

        services.AddWorkerServiceConfigurations(configurationManager);

        services
            .AddLogger()
            .AddDataAccessLayer()
            .AddServiceBasicsForWorkerService()
            .AddCoreLayer()
            .AddPartnerEventsProcessor();

        services.AddSingleton<IJobManagementStorageAccountBuilder, JobManagementStorageAccountBuilder>();
        services.AddScoped<IJobManager, JobManager>();
        services.AddHostedService<WorkerService>();
    }
}
