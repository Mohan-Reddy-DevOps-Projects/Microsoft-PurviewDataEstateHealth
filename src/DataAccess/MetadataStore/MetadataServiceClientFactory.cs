// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Net;
using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;

/// <summary>
/// Manages clients that communicate with the Purview metadata service.
/// Clients are reused for a while, then recycled to allow the http client factory
/// to control the lifetime of the http client handlers
/// </summary>
public class MetadataServiceClientFactory : IDisposable
{
    private const string ApiVersion = "2019-11-01-preview";
    private readonly MetadataServiceConfiguration config;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IDataEstateHealthLogger logger;
    private ProjectBabylonMetadataClient instance;
    private ProjectBabylonMetadataClient previousInstance;
    private long instanceCreationTime;
    private bool disposedValue;

    /// <summary>
    /// Object reference to use for locking
    /// </summary>
    private readonly object lockObj = new object();

    /// <summary>
    /// The name of the metadata service http client to be referenced by the factory
    /// </summary>
    public static string HttpClientName { get; } = "MetadataServiceClient";

    /// <summary>
    /// How often to recycle instance. Matches the client handler lifetime
    /// </summary>
    public static TimeSpan ClientLifetime { get; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="config">Metadata client configuration</param>
    /// <param name="httpClientFactory">Http client factory</param>
    /// <param name="logger">Logger</param>
    public MetadataServiceClientFactory(IOptions<MetadataServiceConfiguration> config, IHttpClientFactory httpClientFactory, IDataEstateHealthLogger logger)
    {
        this.config = config.Value;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Get the current metadata client
    /// </summary>
    /// <returns>Metadata service client</returns>
    public ProjectBabylonMetadataClient GetClient()
    {
        long lastCreationTime = Interlocked.Read(ref this.instanceCreationTime);
        if (lastCreationTime + ClientLifetime.Ticks < DateTime.UtcNow.Ticks || this.instance == null)
        {
            lock (this.lockObj)
            {
                // Within the lock there's no need to use Interlocked to read the value
                if (this.instanceCreationTime + ClientLifetime.Ticks < DateTime.UtcNow.Ticks || this.instance == null)
                {
                    this.previousInstance?.Dispose();
                    this.previousInstance = this.instance;
                    HttpClient httpClient = this.httpClientFactory.CreateClient(HttpClientName);
                    httpClient.DefaultRequestVersion = HttpVersion.Version20;
                    this.instance = new ProjectBabylonMetadataClient(httpClient, true)
                    {
                        BaseUri = new Uri($"https://{this.config.Endpoint}"),
                        ApiVersion = ApiVersion,
                    };

                    Interlocked.Exchange(ref this.instanceCreationTime, DateTime.UtcNow.Ticks);
                    this.logger.LogInformation("MetadataServiceClientFactory|New metadata service client created");
                }
            }
        }

        return this.instance;
    }

    #region IDisposable Support

    /// <summary>
    /// Dispose effective implementation
    /// </summary>
    /// <param name="disposing">True when invoked from Dispose, false from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.instance?.Dispose();
                this.previousInstance?.Dispose();
            }

            this.disposedValue = true;
        }
    }

    /// <summary>
    /// Dispose of the factory
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
