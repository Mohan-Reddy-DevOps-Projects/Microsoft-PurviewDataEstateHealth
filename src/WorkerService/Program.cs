// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.GetCurrentDirectory()
});

Startup.Configure(builder.Services, builder.Configuration);

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
