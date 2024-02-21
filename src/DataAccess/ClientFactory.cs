// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Rest;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

/// <summary>
/// Manages clients that communicate using an http service client.
/// Clients are reused for a while, then recycled to allow the http client factory
/// to control the lifetime of the http client handlers
/// </summary>
public abstract class ClientFactory<T> : IDisposable
    where T : ServiceClient<T>
{
    /// <summary>
    /// Object reference to use for locking
    /// </summary>
    private readonly object lockObj = new object();

    /// <summary>
    /// The name of the http client to be referenced by the factory
    /// </summary>
    protected abstract string ClientName { get; }

    /// <summary>
    /// How often to recycle instance. Matches the client handler lifetime
    /// </summary>
    public static TimeSpan ClientLifetime { get; } = TimeSpan.FromHours(1);

    private T previousInstance;
    private T instance;
    private long instanceCreationTime;
    private bool disposedValue;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IDataEstateHealthRequestLogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientFactory{T}"/> class.
    /// </summary>
    public ClientFactory(
        IHttpClientFactory httpClientFactory,
        IDataEstateHealthRequestLogger logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the client
    /// </summary>
    public T GetClient()
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
                    HttpClient httpClient = this.httpClientFactory.CreateClient(this.ClientName);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestVersion = HttpVersion.Version20;

                    this.instance = this.ConfigureClient(httpClient);

                    Interlocked.Exchange(ref this.instanceCreationTime, DateTime.UtcNow.Ticks);
                    this.logger.LogInformation($"ClientFactory|New client created: {this.ClientName}");
                }
            }
        }

        Interlocked.Exchange(ref this.instanceCreationTime, DateTime.UtcNow.Ticks);
        this.logger?.LogInformation($"New client created: {this.ClientName}");

        return this.instance;
    }

    protected abstract T ConfigureClient(HttpClient httpClient);

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
