// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using AspNetCore.HttpOverrides;
using AspNetCore.OData;
using AspNetCore.OData.NewtonsoftJson;
using AspNetCore.Server.Kestrel.Core;
using AspNetCore.Server.Kestrel.Https;
using Common;
using Configurations;
using Core;
using DataAccess;
using DEH.Application;
using DEH.Infrastructure;
using Extensions;
using Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.BusinessLogic;
using Microsoft.Purview.DataEstateHealth.DHConfigurations;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHModels;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Newtonsoft.Json;
using OpenTelemetry.Audit.Geneva;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Environment = System.Environment;
using OperationType = OpenTelemetry.Audit.Geneva.OperationType;

/// <summary>
///     The Data Estate Health API service.
/// </summary>
public class Program
{
    private const string CorsPolicyName = "AllowOrigin";

    /// <summary>
    ///     Main entry point for the data access service.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (!builder.Environment.IsDevelopment())
        {
            SetAksConfiguration(builder);
        }

        builder.WebHost.ConfigureKestrel((hostingContext, options) =>
        {
            ConfigurePortsAndSsl(hostingContext, options, builder);
        });

        var genevaConfiguration = builder.Configuration.GetSection("geneva")
            .Get<GenevaConfiguration>();

        var serviceConfiguration = builder.Configuration.GetSection("service")
            .Get<ServiceConfiguration>();
        var environmentConfiguration = builder.Configuration.GetSection("environment")
            .Get<EnvironmentConfiguration>();

        builder.Logging.AddOltpExporter(builder.Environment.IsDevelopment(), genevaConfiguration, environmentConfiguration);

        // Add services to the container.
        builder.Services
            .AddApiVersioning(
                //o =>
                //{
                //    o.ReportApiVersions = true;
                //    o.AssumeDefaultVersionWhenUnspecified = true;
                //    o.DefaultApiVersion = new ApiVersion(new DateOnly(2023,10,1), "preview");
                //    o.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                //}
            )
            .AddMvc();
        builder.Services.AddMvc(config => config.Filters.Add<RequestAuditFilter>());

        builder.Services
            .AddLogger(genevaConfiguration, serviceConfiguration, environmentConfiguration, builder.Environment.IsDevelopment())
            .AddApiServiceConfigurations(builder.Configuration)
            .AddApiServices()
            .AddCoreLayer(builder.Configuration)
            .AddDataAccessLayer()
            .AddServiceBasicsForApiService();

        builder.Services.AddHttpContextAccessor();
        builder.AddApplicationSecurityControls();
        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            })
            .AddOData(opt =>
            {
                opt.EnableNoDollarQueryOptions = true;
                opt.Filter()
                    .OrderBy();
            })
            .AddODataNewtonsoftJson();

        builder.Services.AddCertificateForwarding(options =>
        {
            options.CertificateHeader = "X-Forwarded-Client-Cert";
            options.HeaderConverter = CertificateHeaderConverter.Convert;
        });

        builder.Services.AddSingleton(environmentConfiguration);
        builder.Services.AddDHConfigurations(builder.Configuration);
        builder.Services.SetupDHModelsServices();
        builder.Services.SetupDHDataAccessServices();
        builder.Services.SetupBusinessLogicServices();

        builder.Services.SetupDQServices(builder.Configuration);
        builder.Services.AddCatalogDependencies(builder.Configuration);
        builder.Services.AddJobRunLogsDependencies(builder.Configuration);
        builder.Services.AddApplication();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        await Initialize(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
        }
        else
        {
            app.UseCertificateForwarding();
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        var logger = app.Services.GetRequiredService<IDataEstateHealthRequestLogger>();
        var requestContextAccessor = app.Services.GetRequiredService<IRequestContextAccessor>();
        var envConfig = app.Services.GetRequiredService<IOptions<EnvironmentConfiguration>>();
        app.ConfigureExceptionHandler(logger, envConfig, requestContextAccessor);

        // The readiness probe for the AKS pod
        var serverConfig = app.Services.GetRequiredService<IOptions<ServiceConfiguration>>()
            .Value;
        app.UseHealthChecks(serverConfig.ReadinessProbePath, serverConfig.ApiServiceReadinessProbePort.Value);

        app.UseHttpsRedirection()
            .UseRouting()
            .UseCors(CorsPolicyName)
            .UseAuthentication()
            .UseAuthorization()
            .UseMiddleware<MdmMiddleware>()
            .UseApiVersionGuard();

        app.MapControllers();

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
            var readinessCheck = app.Services.GetRequiredService<ServiceHealthCheck>();
            readinessCheck.Initialized = true;

            var logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
            var environmentConfiguration = serviceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>()
                .Value;

            string appId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")!;
            logger.LogInformation("ContainerApp.ApplicationId = {AppId}", appId);

            logger.LogInformation($"ApiService started successfully for versions {String.Join(", ", environmentConfiguration.PermittedApiVersions)}");
            logger.LogAudit(AuditOperation.startup, OperationType.Read, OperationResult.Success, "serviceStartup", "NA");
        });

        await app.RunAsync();
    }

    private static async Task Initialize(WebApplication app)
    {
        try
        {
            // Initialize client certificate cache
            var certificateLoaderService = app.Services.GetRequiredService<ICertificateLoaderService>();
            await certificateLoaderService.InitializeAsync();

            // Initialize the exposure control client
            var exposureControlClient = app.Services.GetRequiredService<IExposureControlClient>();
            await exposureControlClient.Initialize();

            // Initialize PowerBI service
            var powerBIProvider = app.Services.GetService<PowerBIProvider>();
            await powerBIProvider.PowerBIService.Initialize();

            // Initialize synapse service
            var serverlessPoolClient = app.Services.GetService<IServerlessPoolClient>();
            await serverlessPoolClient.Initialize();

            // Initialize metadata service
            var metadataService = app.Services.GetService<IMetadataAccessorService>();
            metadataService.Initialize();

            // Initialize cache
            var cacheManager = app.Services.GetService<ICacheManager>();
            cacheManager.Initialize();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<IDataEstateHealthRequestLogger>();
            logger.LogCritical("Failed to initialize services during startup", ex);
            throw;
        }
    }

    private static void ConfigurePortsAndSsl(WebHostBuilderContext hostingContext, KestrelServerOptions options, WebApplicationBuilder builder)
    {
        if (hostingContext.HostingEnvironment.IsDevelopment())
        {
            ConfigureKestrelServerForDevelopment(options);
        }
        else
        {
            ConfigureKestrelServerForProduction(options, builder);
        }
    }

    private static void ConfigureKestrelServerForProduction(KestrelServerOptions options, WebApplicationBuilder builder)
    {
        var serverConfig = options.ApplicationServices.GetService<IOptions<ServiceConfiguration>>()
            .Value;

        if (serverConfig.ApiServiceReadinessProbePort.HasValue)
        {
            options.Listen(
                IPAddress.IPv6Any,
                serverConfig.ApiServiceReadinessProbePort.Value);
        }

        options.Listen(
            IPAddress.IPv6Any,
            serverConfig.ApiServicePort.Value);
    }

    private static void ConfigureKestrelServerForDevelopment(KestrelServerOptions options)
    {
        var serverConfig = options.ApplicationServices.GetService<IOptions<ServiceConfiguration>>()
            .Value;

        options.ListenAnyIP(
            serverConfig.ApiServicePort.Value,
            listenOptions => listenOptions.UseHttps(httpsOptions =>
            {
                httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                httpsOptions.AllowAnyClientCertificate();
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            }));

        if (serverConfig.ApiServiceReadinessProbePort.HasValue)
        {
            options.Listen(
                IPAddress.IPv6Any,
                serverConfig.ApiServiceReadinessProbePort.Value);
        }
    }

    private static void SetAksConfiguration(WebApplicationBuilder builder)
    {
        const string appSettingsEnvVar = "APP_SETTINGS_JSON";
        string appSettingsJson = Environment.GetEnvironmentVariable(appSettingsEnvVar);
        if (appSettingsJson == null)
        {
            throw new InvalidOperationException($"environment variable '{appSettingsEnvVar}' was not found");
        }

        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));
    }
}