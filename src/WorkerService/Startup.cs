// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using System;
using System.IO;
using System.Text;

/// <summary>
/// Worker service startup.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Sets up configuration and DI services.
    /// </summary>
    /// <param name="builder">The builder</param>
    public static void Configure(WebApplicationBuilder builder)
    {
        builder.Services.AddWorkerServiceConfigurations(builder.Configuration);

        if (!builder.Environment.IsDevelopment())
        {
            ConfigureKestrelServerForProduction(builder);
        }

        var genevaConfiguration = builder.Configuration.GetSection("geneva").Get<GenevaConfiguration>();

        var serviceConfiguration = builder.Configuration.GetSection("service").Get<ServiceConfiguration>();

        var environmentConfiguration = builder.Configuration.GetSection("environment").Get<EnvironmentConfiguration>();

        builder.Services
            .AddLogger(genevaConfiguration, serviceConfiguration, environmentConfiguration, builder.Environment.IsDevelopment())
            .AddDataAccessLayer()
            .SetupDHDataAccessServices()
            .AddServiceBasicsForWorkerService()
            .AddCoreLayer(builder.Configuration)
            .AddPartnerEventsProcessor();

        builder.Services.AddHostedService<WorkerService>();
    }

    private static void ConfigureKestrelServerForProduction(WebApplicationBuilder builder)
    {
        string appSettingsJson = System.Environment.GetEnvironmentVariable("APP_SETTINGS_JSON") ?? throw new Exception("environment variable 'APP_SETTINGS_JSON' is missing");
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));
    }
}
