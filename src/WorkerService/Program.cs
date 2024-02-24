// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.WorkerService;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Extensions.Azure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.GetCurrentDirectory()
});

Startup.Configure(builder);

var environmentConfiguration = builder.Configuration.GetSection("environment").Get<EnvironmentConfiguration>();

builder.Logging.AddOltpExporter(builder.Environment.IsDevelopment(), environmentConfiguration);


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
await Initialize(app);

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


static async Task Initialize(WebApplication app)
{
    try
    {
        // Initialize client certificate cache
        ICertificateLoaderService certificateLoaderService = app.Services.GetRequiredService<ICertificateLoaderService>();
        await certificateLoaderService.InitializeAsync();

        // Initialize the exposure control client
        IExposureControlClient exposureControlClient = app.Services.GetRequiredService<IExposureControlClient>();
        await exposureControlClient.Initialize();

        // Initialize PowerBI service
        PowerBIProvider powerBIProvider = app.Services.GetService<PowerBIProvider>();
        await powerBIProvider.PowerBIService.Initialize();

        // Initialize synapse service
        IServerlessPoolClient serverlessPoolClient = app.Services.GetService<IServerlessPoolClient>();
        await serverlessPoolClient.Initialize();

        // Initialize metadata service
        IMetadataAccessorService metadataService = app.Services.GetService<IMetadataAccessorService>();
        metadataService.Initialize();

        // Initialize cache
        ICacheManager cacheManager = app.Services.GetService<ICacheManager>();
        cacheManager.Initialize();
    }
    catch (Exception ex)
    {
        IDataEstateHealthRequestLogger logger = app.Services.GetRequiredService<IDataEstateHealthRequestLogger>();
        logger.LogCritical("Failed to initialize services during startup", ex);
        throw;
    }
}
