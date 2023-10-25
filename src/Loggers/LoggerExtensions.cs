// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

/// <summary>
/// Add logger services
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Add the logger services to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDataEstateHealthLogger, DataEstateHealthLogger>();
        serviceCollection.AddScoped<IDataEstateHealthRequestLogger, DataEstateHealthLogger>();

        return serviceCollection;
    }

    /// <summary>
    /// Adds the OTLP exporter.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="isDevelopmentEnvironment"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddOltpExporter(this ILoggingBuilder builder, bool isDevelopmentEnvironment)
    {
        builder.ClearProviders();
        builder.AddOpenTelemetry(options =>
        {
            if (isDevelopmentEnvironment)
            {
                options.AddConsoleExporter();
            }
            else
            {
                const string loggingEndpointEnvVar = "FIRSTPARTY_LOGGING_GRPC_ENDPOINT";
                string loggingEndpoint = Environment.GetEnvironmentVariable(loggingEndpointEnvVar);
                if (loggingEndpoint == null)
                {
                    throw new Exception($"environment variable '{loggingEndpointEnvVar}' was not found");
                }

                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                options.AddOtlpExporter(otelOptions =>
                {
                    otelOptions.Endpoint = new Uri($"http://{loggingEndpoint}");
                    otelOptions.Protocol = OtlpExportProtocol.Grpc;
                });
            }
        });
        
        return builder;
    }
}
