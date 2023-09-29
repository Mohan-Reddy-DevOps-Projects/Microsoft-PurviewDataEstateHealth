// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataAccess.ApiService;

using System.Net;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using System.Text;

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

        // Add services to the container.
        builder.Services.AddApiVersioning();

        var credentialOptions = new DefaultAzureCredentialOptions();
        credentialOptions.Retry.Mode = RetryMode.Fixed;
        credentialOptions.Retry.Delay = TimeSpan.FromSeconds(15);
        credentialOptions.Retry.MaxRetries = 12;
        credentialOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(100);
        credentialOptions.ExcludeManagedIdentityCredential = builder.Environment.IsDevelopment();
        credentialOptions.ExcludeWorkloadIdentityCredential = builder.Environment.IsDevelopment();

        TokenCredential azureCredentials = new DefaultAzureCredential(credentialOptions);

        builder.Services
            .AddApiServiceConfigurations(builder.Configuration)
            .AddApiServices()
            .AddCoreLayer(azureCredentials)
            .AddDataAccessLayer();

        builder.Services
            .AddScoped<CertificateValidationService>()
            .AddAuthentication()
            .AddCertificateAuthentication();

        builder.Services
            .AddAuthentication();

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
                opt.Filter().OrderBy();
            })
            .AddODataNewtonsoftJson();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .AddLogger();

        var app = builder.Build();

        // Initialize client certificate cache
        ICertificateLoaderService certificateLoaderService = app.Services.GetRequiredService<ICertificateLoaderService>();
        await certificateLoaderService.InitializeAsync();

        // Initialize PowerBI service
        IPowerBIService powerBIService = app.Services.GetService<IPowerBIService>();
        await powerBIService.Initialize();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        // The readiness probe for the AKS pod
        ServiceConfiguration serverConfig = app.Services.GetRequiredService<IOptions<ServiceConfiguration>>().Value;
        app.UseHealthChecks(serverConfig.ReadinessProbePath, serverConfig.ApiServiceReadinessProbePort.Value);

        app.UseHttpsRedirection()
            .UseRouting()
            .UseCors(CorsPolicyName)
            .UseAuthentication()
            .UseAuthorization();

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
                ICertificateLoaderService certificateLoaderService = options.ApplicationServices.GetService<ICertificateLoaderService>();
                EnvironmentConfiguration environmentConfiguration = options.ApplicationServices.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;

                SslConfiguration sslConfiguration = options.ApplicationServices
                    .GetRequiredService<IOptions<SslConfiguration>>()
                    .Value;

                var httpsConnectionAdapterOptions =
                    new HttpsConnectionAdapterOptions
                    {
                        ClientCertificateMode = ClientCertificateMode.RequireCertificate,
                        // Restrict to use TLS 1.2 or greater and set the preference order of cipher suites
                        ServerCertificateSelector = (connectionContext, name) =>
                        {
                            X509Certificate2 sslCertificate = certificateLoaderService.LoadAsync(
                                sslConfiguration.CertificateName).GetAwaiter().GetResult();

                            return sslCertificate;
                        },
                        OnAuthenticate = (conContext, sslAuthOptions) =>
                        {
                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                sslAuthOptions.CipherSuitesPolicy =
                                    TlsProtocols.CipherSuitesPolicy;
                            }
                        }
                    };
                listenOptions.UseHttps(httpsConnectionAdapterOptions);
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
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
        string appSettingsJson = Environment.GetEnvironmentVariable(appSettingsEnvVar);
        if (appSettingsJson == null)
        {
            throw new InvalidOperationException($"environment variable '{appSettingsEnvVar}' was not found");
        }

        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));
    }
}
