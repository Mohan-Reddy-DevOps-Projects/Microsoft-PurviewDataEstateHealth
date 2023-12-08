// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Environment = System.Environment;
using HttpProtocol = AspNetCore.Http.HttpProtocol;

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
    /// <param name="isDevEnvironment"></param>
    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection,
        GenevaConfiguration genevaConfiguration, ServiceConfiguration serviceConfiguration, bool isDevEnvironment)
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
           serviceVersion: GetSanitizedEnvironmentVariable("ROLE_VERSION"),
           serviceInstanceId: GetSanitizedEnvironmentVariable("NODE_NAME"));

        serviceCollection.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithTracing(builder =>
            {
                builder.SetSampler(new AlwaysOnSampler())
                    .AddSource(DataEstateHealthOtelInstrumentation.InstrumentationName)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;

                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                           activity.AddTag(RootTraceID, activity.GetRootId());

                            if (request.RequestUri is not null)
                            {
                                activity.SetTag("http.url", request.RequestUri.GetComponents(UriComponents.AbsoluteUri ^ UriComponents.Query, UriFormat.Unescaped));
                            }
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
                            activity.AddTag("http.flavor", GetHttpFlavor(request.Protocol));
                            activity.AddTag("http.scheme", request.Scheme);
                            activity.AddTag("http.request_content_length", request.ContentLength);
                            activity.AddTag("http.request_content_type", request.ContentType);
                            activity.AddTag("TenantId", request.Headers.GetFirstOrDefault(HeaderClientTenantId));
                            activity.AddTag("AccountId", request.Headers.GetFirstOrDefault(AccountIdHeader));
                            activity.AddTag("ApiVersion", request.GetFirstOrDefaultQuery(ParameterApiVersion));
                            activity.AddTag("ErrorType", ErrorType.GetErrorType(context?.Response?.StatusCode));
                            activity.AddTag("CorrelationId", request.Headers.GetFirstOrDefault(HeaderCorrelationRequestId));
                            activity.AddTag(RootTraceID, activity.GetRootId());
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.AddTag("http.response_content_length", response.ContentLength);
                            activity.AddTag("http.response_content_type", response.ContentType);
                            activity.AddTag("ErrorType", ErrorType.GetErrorType(response?.StatusCode));
                            activity.AddTag(RootTraceID, activity.GetRootId());
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
                builder.AddMeter(DataEstateHealthOtelInstrumentation.InstrumentationName)
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation(options => options.Enrich = EnrichMetrics)
                    .AddHttpClientInstrumentation();

                if (!isDevEnvironment)
                {
                    var prepopulatedDimensions = GetPrepopulatedFields();

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

        return value;
    }

    private static Dictionary<string, object> GetPrepopulatedFields()
    {
        return new Dictionary<string, object>
        {
            ["PodName"] = GetSanitizedEnvironmentVariable("POD_NAME"),
            ["cloud.role"] = GetRoleName(),
            ["cloud.roleInstance"] = GetSanitizedEnvironmentVariable("NODE_NAME"),
            ["cloud.roleVer"] = GetSanitizedEnvironmentVariable("ROLE_VERSION")
        };
    }

    private static void EnrichMetrics(string name, HttpContext context, ref TagList tags)
    {
        tags.Add("TenantId", context?.Request?.Headers.GetFirstOrDefault(HeaderClientTenantId));
        tags.Add("AccountId", context?.Request?.Headers.GetFirstOrDefault(AccountIdHeader));
        tags.Add("ApiVersion", context?.Request?.GetFirstOrDefaultQuery(ParameterApiVersion));
        tags.Add("ErrorType", ErrorType.GetErrorType(context?.Response?.StatusCode));
        tags.Add("CorrelationId", context?.Request?.Headers.GetFirstOrDefault(HeaderCorrelationRequestId));
    }

    private static string GetHttpFlavor(string protocol)
    {
        if (HttpProtocol.IsHttp10(protocol))
        {
            return "1.0";
        }
        else if (HttpProtocol.IsHttp11(protocol))
        {
            return "1.1";
        }
        else if (HttpProtocol.IsHttp2(protocol))
        {
            return "2.0";
        }
        else if (HttpProtocol.IsHttp3(protocol))
        {
            return "3.0";
        }

        throw new InvalidOperationException($"Protocol {protocol} not recognised.");
    }

    private static string GetRoleName()
    {
        return "DGHealth" + GetSanitizedEnvironmentVariable("CONTAINER_NAME");
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
