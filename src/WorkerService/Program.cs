// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.WorkerService;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

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
});

WebApplication app = builder.Build();

app.Lifetime.ApplicationStarted.Register(
    () =>
    {
        // TODO(zachmadsen): Add logging here to mark successful startup.
    });

await app.RunAsync();
