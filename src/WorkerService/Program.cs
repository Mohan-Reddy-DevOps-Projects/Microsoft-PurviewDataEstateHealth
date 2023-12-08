// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.WorkerService;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Core;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.GetCurrentDirectory()
});

builder.Logging.AddOltpExporter(builder.Environment.IsDevelopment());

Startup.Configure(builder);

builder.WebHost.ConfigureKestrel((hostingContext, options) =>
{
    ServiceConfiguration serverConfig = options.ApplicationServices.GetService<IOptions<ServiceConfiguration>>().Value;

    options.Listen(
        IPAddress.IPv6Any,
        serverConfig.WorkerServicePort);

    if (serverConfig.WorkerServiceReadinessProbePort.HasValue)
    {
        options.Listen(
            IPAddress.IPv6Any,
            serverConfig.WorkerServiceReadinessProbePort.Value);
    }
});

WebApplication app = builder.Build();

var serviceConfig = app.Services.GetRequiredService<IOptions<ServiceConfiguration>>().Value;

app.UseHealthChecks(serviceConfig.ReadinessProbePath, serviceConfig.WorkerServiceReadinessProbePort.Value);

app.Lifetime.ApplicationStarted.Register(
    () =>
    {
        IServiceProvider serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
        ServiceHealthCheck readinessCheck = app.Services.GetRequiredService<ServiceHealthCheck>();
        readinessCheck.Initialized = true;

        IDataEstateHealthRequestLogger logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        EnvironmentConfiguration environmentConfiguration = serviceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;
        logger.LogInformation($"Worker service started successfully.");
    });

await app.RunAsync();
