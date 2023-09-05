// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

/// <summary>
/// Worker service startup.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="webHostEnvironment">Worker service environment.</param>
    public Startup(IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        this.Configuration = configuration;
        this.WebHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Worker service environment.
    /// </summary>
    public IWebHostEnvironment WebHostEnvironment { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        TokenCredential azureCredentials;
        if (this.WebHostEnvironment.IsDevelopment())
        {
            //When debugging inside a Docker container, we can't authenticate to KeyVault using the Visual Studio identity.
            //This workaround fetches an access token that's retrieved during a debug build.
            //It should be compatible with all methods of debugging - IISExpress, Kestrel, and Docker.

            string accessTokenText;
            using (var streamReader = new StreamReader("debugAccessToken.json", Encoding.UTF8))
            {
                accessTokenText = streamReader.ReadToEnd();
            }
            dynamic accessTokenConfig = JsonConvert.DeserializeObject(accessTokenText);

            string token = accessTokenConfig.Token;
            var expiresOn = Convert.ToDateTime(accessTokenConfig.ExpiresOn);
            azureCredentials = DelegatedTokenCredential.Create(
                (_, _) => new AccessToken(token, expiresOn),
                (_, _) => ValueTask.FromResult(new AccessToken(token, expiresOn)));
        }
        else
        {
            var credentialOptions = new DefaultAzureCredentialOptions();
            credentialOptions.Retry.Mode = RetryMode.Fixed;
            credentialOptions.Retry.Delay = TimeSpan.FromSeconds(15);
            credentialOptions.Retry.MaxRetries = 12;
            credentialOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(100);

            azureCredentials = new DefaultAzureCredential(credentialOptions);
        }

        var serverConfiguration = this.Configuration.GetSection("serverConfiguration").Get<ServerConfiguration>();
  
        services.AddHostedService<WorkerService>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var serverConfig = app.ApplicationServices.GetService<IOptions<ServerConfiguration>>().Value;
        app.UseHealthChecks(serverConfig.ReadinessProbePath, serverConfig.WorkerServiceReadinessProbePort.Value);

        hostApplicationLifetime.ApplicationStarted.Register(
            () =>
            {
                IServiceProvider serviceProvider = app.ApplicationServices.GetRequiredService<IServiceProvider>();

                IDataEstateHealthLogger dataEstateHealthLogger = serviceProvider.GetRequiredService<IDataEstateHealthLogger>();
                var environmentConfiguration = serviceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;
                dataEstateHealthLogger.LogInformation($"WorkerService started successfully for versions {string.Join(", ", environmentConfiguration.PermittedApiVersions)}");
            });
    }
}
