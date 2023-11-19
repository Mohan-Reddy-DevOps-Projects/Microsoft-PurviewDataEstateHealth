// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataAccess.ApiService;

using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The Data Estate Health API service.
/// </summary>
public class Program
{
    private const string CorsPolicyName = "AllowOrigin";

    /// <summary>
    /// Main entry point for the data access service.
    /// </summary>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel((hostingContext, options) =>
        {
            ConfigurePortsAndSsl(hostingContext, options, builder);
        });

        builder.Logging.AddOltpExporter(builder.Environment.IsDevelopment());

        // Add services to the container.
        builder.Services.AddApiVersioning();

        builder.Services
            .AddLogger()
            .AddApiServiceConfigurations(builder.Configuration)
            .AddApiServices()
            .AddProvisioningService()
            .AddCoreLayer()
            .AddDataAccessLayer()
            .AddServiceBasicsForApiService();

        builder.Services
            .AddScoped<CertificateValidationService>()
            .AddAuthentication()
            .AddCertificateAuthentication();

        builder.Services
            .AddAuthentication();

        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add(typeof(ServiceExceptionFilter));
            })
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
                opt.Filter().OrderBy();
            })
            .AddODataNewtonsoftJson();

        builder.Services.AddCertificateForwarding(options =>
        {
            options.CertificateHeader = "X-Forwarded-Client-Cert";
            options.HeaderConverter = CertificateHeaderConverter.Convert;
        });
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        WebApplication app = builder.Build();

        await Initialize(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseCertificateForwarding();
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        // The readiness probe for the AKS pod
        ServiceConfiguration serverConfig = app.Services.GetRequiredService<IOptions<ServiceConfiguration>>().Value;
        app.UseHealthChecks(serverConfig.ReadinessProbePath, serverConfig.ApiServiceReadinessProbePort.Value);

        app.UseHttpsRedirection()
            .UseRouting()
            .UseCors(CorsPolicyName)
            .UseAuthentication()
            .UseAuthorization()
            .UseApiVersionGuard();

        app.MapControllers();

        app.Lifetime.ApplicationStarted.Register(
            () =>
            {
                IServiceProvider serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
                ServiceHealthCheck readinessCheck = app.Services.GetRequiredService<ServiceHealthCheck>();
                readinessCheck.Initialized = true;

                IDataEstateHealthLogger logger = serviceProvider.GetRequiredService<IDataEstateHealthLogger>();
                EnvironmentConfiguration environmentConfiguration = serviceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;
                logger.LogInformation($"ApiService started successfully for versions {string.Join(", ", environmentConfiguration.PermittedApiVersions)}");
            });

        await app.RunAsync();
    }

    private static async Task Initialize(WebApplication app)
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
            IPowerBIService powerBIService = app.Services.GetService<IPowerBIService>();
            await powerBIService.Initialize();

            // Initialize synapse service
            IServerlessPoolClient serverlessPoolClient = app.Services.GetService<IServerlessPoolClient>();
            await serverlessPoolClient.Initialize();

            // Initialize metadata service
            IMetadataAccessorService metadataService = app.Services.GetService<IMetadataAccessorService>();
            metadataService.Initialize();
        }
        catch (Exception ex)
        {
            IDataEstateHealthLogger logger = app.Services.GetRequiredService<IDataEstateHealthLogger>();
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
        SetAksConfiguration(builder);

        var serverConfig = options.ApplicationServices.GetService<IOptions<ServiceConfiguration>>().Value;

        if (serverConfig.ApiServiceReadinessProbePort.HasValue)
        {
            options.Listen(
                IPAddress.IPv6Any,
                serverConfig.ApiServiceReadinessProbePort.Value);
        }

        options.Listen(
            IPAddress.IPv6Any,
            serverConfig.ApiServicePort.Value,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });
    }

    private static void ConfigureKestrelServerForDevelopment(KestrelServerOptions options)
    {
        var serverConfig = options.ApplicationServices.GetService<IOptions<ServiceConfiguration>>().Value;

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
        string appSettingsJson = System.Environment.GetEnvironmentVariable(appSettingsEnvVar);
        if (appSettingsJson == null)
        {
            throw new InvalidOperationException($"environment variable '{appSettingsEnvVar}' was not found");
        }

        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));
    }
}
