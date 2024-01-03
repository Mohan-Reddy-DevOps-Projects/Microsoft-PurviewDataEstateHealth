// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Environment = System.Environment;

/// <summary>
/// Add logger services
/// </summary>
public static class LoggerExtensions
{
    private const string RootTraceID = "RootTraceId";
    private const string HeaderCorrelationRequestId = "x-ms-correlation-request-id";
    private const string AccountIdHeader = "x-ms-account-id";
    private const string HeaderClientTenantId = "x-ms-client-tenant-id";
    private const string ParameterApiVersion = "api-version";

    /// <summary>
    /// Add the logger services to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="genevaConfiguration"></param>
    /// <param name="serviceConfiguration"></param>
    /// <param name="environmentConfiguration"></param>
    /// <param name="isDevEnvironment"></param>
    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection,
        GenevaConfiguration genevaConfiguration,
        ServiceConfiguration serviceConfiguration,
        EnvironmentConfiguration environmentConfiguration,
        bool isDevEnvironment)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        // Mdm Loggers

        var purviewOtelInstrumentation = new DataEstateHealthOtelInstrumentation();
        serviceCollection.AddSingleton<IOtelInstrumentation>(purviewOtelInstrumentation);

        // Mds Loggers
        serviceCollection.AddSingleton<IRequestContextAccessor, RequestContextAccessor>();
        serviceCollection.AddSingleton<IDataEstateHealthRequestLogger, DataEstateHealthRequestLogger>();

        Action<ResourceBuilder> configureResource = r => r.AddService(
           serviceName: GetRoleName(),
           serviceVersion: GetSanitizedEnvironmentVariable("BUILD_VERSION"),
           serviceInstanceId: GetSanitizedEnvironmentVariable("CONTAINER_APP_REVISION"));

        serviceCollection.AddOpenTelemetry()
            .ConfigureResource(configureResource)
         
            .WithTracing(builder =>
            {
                builder.SetSampler(new AlwaysOnSampler())
                    .AddProcessor(new AddEnvStateActivityProcessor(environmentConfiguration))
                    .AddSource(DataEstateHealthOtelInstrumentation.InstrumentationName)
                    .SetErrorStatusOnException()
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;

                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            if (request.RequestUri is not null)
                            {
                                activity.SetTag("http.url", request.RequestUri.GetComponents(UriComponents.AbsoluteUri ^ UriComponents.Query, UriFormat.Unescaped));

                                activity.SetTag(RootTraceID, activity.GetRootId());
                            }
                        };

                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag(RootTraceID, activity.GetRootId());
                        };
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        //Filter out readiness probe checks
                        options.Filter = (ctx) =>
                        !ctx.Request?.Path.Value.EqualsOrdinalInsensitively(serviceConfiguration.ReadinessProbePath) ?? true &&
                            ctx.Response?.StatusCode == (int)HttpStatusCode.OK;

                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            var context = request.HttpContext;
                            activity.SetTag("http.request_content_length", request.ContentLength);
                            activity.SetTag("http.request_content_type", request.ContentType);
                            activity.SetTag("TenantId", request.Headers.GetFirstOrDefault(HeaderClientTenantId));
                            activity.SetTag("AccountId", request.Headers.GetFirstOrDefault(AccountIdHeader));
                            activity.SetTag("ApiVersion", request.GetFirstOrDefaultQuery(ParameterApiVersion));
                            activity.SetTag("ErrorType", ErrorType.GetErrorType(context?.Response?.StatusCode));
                            activity.SetTag("CorrelationId", request.Headers.GetFirstOrDefault(HeaderCorrelationRequestId));
                            activity.SetTag(RootTraceID, activity.GetRootId());
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response_content_length", response.ContentLength);
                            activity.SetTag("http.response_content_type", response.ContentType);
                            activity.SetTag("ErrorType", ErrorType.GetErrorType(response?.StatusCode));
                            activity.SetTag(RootTraceID, activity.GetRootId());
                        };
                    });

                if (isDevEnvironment)
                {
                    builder.AddConsoleExporter();
                }
                else
                {
                    builder.AddOtlpExporter(options =>
                    {
                        string tracingEndpoint = Environment.GetEnvironmentVariable("FIRSTPARTY_TRACING_GRPC_ENDPOINT");
                        options.Endpoint = new Uri($"http://{tracingEndpoint}");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .ConfigureResource(configureResource)
                    .AddMeter(DataEstateHealthOtelInstrumentation.InstrumentationName)
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!isDevEnvironment)
                {
                    var prepopulatedDimensions = GetPrepopulatedFields(environmentConfiguration);

                    if (genevaConfiguration != null)
                    {
                        foreach (var dimension in genevaConfiguration.DefaultDimensions)
                        {
                            prepopulatedDimensions.TryAdd(dimension.Key, dimension.Value);
                        }
                    }

                    builder.AddOtlpExporter(options =>
                    {
                        var metricEndpoint = Environment.GetEnvironmentVariable("FIRSTPARTY_METRIC_GRPC_ENDPOINT");
                        options.Endpoint = new Uri($"http://{metricEndpoint}");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    })
                    .AddInMemoryExporter(new List<Metric>(), metricReaderOptions =>
                        metricReaderOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta
                    );
                }
            });

        return serviceCollection;
    }

    private static string GetSanitizedEnvironmentVariable(string name)
    {
        return SanitizeDimensionValue(Environment.GetEnvironmentVariable(name));
    }
    private static string SanitizeDimensionValue(string value)
    {
        // truncate strings too long for MDM dimensions
        if (!string.IsNullOrEmpty(value))
        {
            value = value[..Math.Min(value.Length, 255)];
        }

        if (string.IsNullOrEmpty(value))
        {
            value = "_empty";
        }

        return value;
    }

    private static Dictionary<string, object> GetPrepopulatedFields(EnvironmentConfiguration environmentConfiguration)
    {
        return new Dictionary<string, object>
        {
            ["cloud.role"] = GetRoleName(),
            ["cloud.roleInstance"] = GetSanitizedEnvironmentVariable("CONTAINER_APP_REVISION"),
            ["cloud.roleVer"] = GetSanitizedEnvironmentVariable("BUILD_VERSION"),
            ["env.name"] = environmentConfiguration.Environment.ToString(),
            ["RoleLocation"] = environmentConfiguration.Location,
            ["ServiceId"] = GetSanitizedEnvironmentVariable("SERVICE_ID")
        };
    }

    private static string GetRoleName()
    {
        return GetSanitizedEnvironmentVariable("CONTAINER_APP_NAME");
    }

    /// <summary>
    /// Adds the OTLP exporter.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="isDevelopmentEnvironment"></param>
    /// <param name="environmentConfiguration"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddOltpExporter(this ILoggingBuilder builder,
        bool isDevelopmentEnvironment,
        EnvironmentConfiguration environmentConfiguration)
    {
        builder.ClearProviders();
        builder.AddOpenTelemetry(options =>
        {
            options.AddProcessor(new AddEnvStateLogProcessor(environmentConfiguration));
            options.IncludeScopes = true;
            options.ParseStateValues = true;

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
