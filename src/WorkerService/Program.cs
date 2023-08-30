// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Environment = System.Environment;
using Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.GetCurrentDirectory()
});

var startup = new Startup(builder.Configuration, builder.Environment);

try
{
    string configurationFolder = "config";
    string[] configurationFiles = { "appsettings.json" };

    if (builder.Environment.IsDevelopment())
    {
        configurationFolder = ".";
        configurationFiles = Environment.GetEnvironmentVariable("CONFIG_FILE")
            ?.Split(";");
    }

    if (configurationFiles != null)
    {
        foreach (string file in configurationFiles)
        {
            string basePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                configurationFolder);
            string configFile = Path.Combine(basePath, file);
            if (File.Exists(configFile))
            {
                builder.Configuration.SetBasePath(basePath);
                builder.Configuration.AddJsonFile(configFile);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unable to find configuration file: {configFile}");
            }
        }
    }

    builder.WebHost.ConfigureKestrel((hostingContext, options) =>
    {

        var serverConfig = options.ApplicationServices.GetService<IOptions<ServerConfiguration>>().Value;

        if (serverConfig.WorkerServiceReadinessProbePort.HasValue)
        {
            options.Listen(
                IPAddress.IPv6Any,
                serverConfig.WorkerServiceReadinessProbePort.Value);
        }

        options.Listen(
                IPAddress.IPv6Any,
                serverConfig.WorkerServicePort.Value);
    });

    startup.ConfigureServices(builder.Services);

    builder.Logging.ClearProviders();

    var app = builder.Build();

    startup.Configure(app, builder.Environment, app.Lifetime);

    await app.RunAsync();
}
catch (Exception exception)
{
    Console.WriteLine(
    JsonConvert.SerializeObject(
            new
            {
                Message = "Unhandled exception during service start up",
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace
            }));

    throw;
}
