// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Manages clients that communicate with Power BI.
/// Clients are reused for a while, then recycled to allow the http client factory
/// to control the lifetime of the http client handlers
/// </summary>
internal class PowerBIFactory : IDisposable
{
    private static readonly TimeSpan maxCacheBlockingTime = TimeSpan.FromSeconds(10);
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly PowerBIAuthConfiguration authConfiguration;
    /*private readonly ICounter callCounter;
    private readonly ICounter errorCounter;*/
    private readonly IAadAppTokenProviderService<PowerBIAuthConfiguration> aadService;
    private PowerBIClient instance;
    private PowerBIClient previousInstance;
    private long accessTokenExpirarationTime;
    private long instanceCreationTime;
    private bool disposedValue;

    /// <summary>
    /// Object reference to use for locking
    /// </summary>
    private readonly SemaphoreSlim cacheSemaphore = new(1);

    /// <summary>
    /// The name of the metadata service http client to be referenced by the factory
    /// </summary>
    public static string HttpClientName { get; } = "PowerBIClient";

    /// <summary>
    /// How often to recycle instance. Matches the client handler lifetime
    /// </summary>
    public static TimeSpan ClientLifetime { get; } = TimeSpan.FromMinutes(31);

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="authConfiguration"></param>
    /// <param name="aadService"></param>
    public PowerBIFactory(
        IDataEstateHealthRequestLogger logger,
        IOptions<PowerBIAuthConfiguration> authConfiguration,
        IAadAppTokenProviderService<PowerBIAuthConfiguration> aadService)
        // IRequestContextAccessor requestContextAccessor,
        // IMetricsWriter metricsWriter)
    {
        this.logger = logger;
        this.authConfiguration = authConfiguration.Value;
        this.aadService = aadService;
        /*this.callCounter = this.logger.GetCounter(metricsWriter, requestContextAccessor, "PowerBIInitialize");
        this.errorCounter = this.logger.GetCounter(metricsWriter, requestContextAccessor, "PowerBIInitializeError");*/
    }

    /// <summary>
    /// Get the current client
    /// </summary>
    /// <returns>Power Bi client</returns>
    public async Task<PowerBIClient> GetClientAsync(CancellationToken cancellationToken)
    {
        long lastCreationTime = Interlocked.Read(ref this.instanceCreationTime);
        long accessTokenExpirarationTime = Interlocked.Read(ref this.accessTokenExpirarationTime);
        bool tokenExpired = accessTokenExpirarationTime - TimeSpan.FromMinutes(this.authConfiguration.RenewBeforeExpiryInMinutes).Ticks < DateTime.UtcNow.Ticks;
        if (tokenExpired || lastCreationTime + ClientLifetime.Ticks < DateTime.UtcNow.Ticks || this.instance == null)
        {
            try
            {
                await this.cacheSemaphore.WaitAsync(maxCacheBlockingTime, cancellationToken);

                // Within the lock there's no need to use Interlocked to read the value
                if (tokenExpired || this.instanceCreationTime + ClientLifetime.Ticks < DateTime.UtcNow.Ticks || this.instance == null)
                {
                    this.previousInstance?.Dispose();
                    this.previousInstance = this.instance;

                    AuthenticationResult accessToken = await this.aadService.GetTokenAsync(this.authConfiguration.Resource, tokenExpired);
                    TokenCredentials tokenCredentials = new(accessToken.AccessToken, TokenUtils.Bearer);
                    this.instance = new PowerBIClient(new Uri(this.authConfiguration.ClientUri), tokenCredentials);

                    Interlocked.Exchange(ref this.instanceCreationTime, DateTime.UtcNow.Ticks);
                    Interlocked.Exchange(ref this.accessTokenExpirarationTime, accessToken.ExpiresOn.Ticks);
                    this.logger.LogInformation($"{HttpClientName}| New client created");
                    // this.callCounter.Increment();
                }
            }
            catch (Exception ex)
            {
                this.instance = null;
                this.logger.LogError($"{HttpClientName}| Error getting client token", ex);
                // this.errorCounter.Increment(ex);
                throw;
            }
            finally
            {
                this.cacheSemaphore.Release();
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
        if (!disposedValue)
        {
            if (disposing)
            {
                this.instance?.Dispose();
                this.previousInstance?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Dispose of the factory
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
