// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.Azure.ProjectBabylon.Caching;
using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest.Azure;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Cache objects that are expensive to retrieve or compute.
/// This applies to objects that must be computed for every request but whose values 
/// do not usually change, or may change rarely such as
/// 1. The account info
/// 2. Access tokens (expiry decided from the token's TTL)
/// Wraps a Cache object from the shared ProjectBabylon.Caching library.
/// </summary>

public class CacheManager : ICacheManager, IDisposable
{
    private const string Tag = "CacheManager";
    private ICache cache;
    private readonly IMemoryCache memoryCache;
    private readonly IDataEstateHealthRequestLogger logger;
    private Timer timer;
    private bool forceMemoryCacheOnly;
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheManager"/> class.
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="logger"></param>
    public CacheManager(IMemoryCache memoryCache, IDataEstateHealthRequestLogger logger)
    {
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (this.cache != null)
        {
            throw new InvalidOperationException("Cache already initialized");
        }
        CacheConfig cacheConfig = new CacheConfig()
        {
            ExternalCacheDisabled = true,
            ExternalCacheSilentErrors = false,
            ExternalCacheKeyNamePrefix = "health",
            CacheResetRecordInterval = TimeSpan.FromMinutes(5.2),
            InMemoryCacheInterval = TimeSpan.FromMinutes(5)
        };

        this.forceMemoryCacheOnly = true;
        cacheConfig.ExternalCacheDisabled = true;

        this.cache = new Cache(memoryCache: this.memoryCache, cacheConfig: cacheConfig);
    }

    /// <inheritdoc/>
    public async Task<T> GetAsync<T>(string cacheKey, string cachePrefix)
    {
        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            CacheResult<T> result = await this.cache.GetCachedRecordAsync<T>(cacheKey, this.forceMemoryCacheOnly);
            sw.Stop();
            bool entityExists = !EqualityComparer<T>.Default.Equals(result.Value, default);
            var location = result.FoundInExternalCache ? "redis" : "memory";
            this.logger.LogTrace($"{Tag}|{cachePrefix} {(entityExists ? $"exists in {location}" : "does not exist in any")} cache. Operation took {sw.ElapsedMilliseconds} ms");
            return result.Value;
        }
        catch (CacheManagerException ex)
        {
            // If Redis cache can't be accessed do not throw (it could cause the caller's operation to fail)
            // Instead return default(T) to indicate the value was not found in the cache, so that the caller can recover
            // by retrieving the object from is source
            this.logger.LogError($"{Tag}|Cache Error for: {cachePrefix}", ex);

            return default;
        }
        catch (Exception exception)
        {
            this.logger.LogError($"{Tag}|Unknown exception for: {cachePrefix}", exception);

            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan duration, string cachePrefix)
    {
        try
        {
            await this.cache.SetCacheValueAsync(cacheKey, value, duration, this.forceMemoryCacheOnly);
        }
        catch (CacheManagerException ex)
        {
            // If Redis cache can't be accessed do not throw (it could cause the caller's operation to fail)
            // Ignore error as the item has already been added to the memory cache.
            this.logger.LogError($"{Tag}|Error for: {cachePrefix}", ex);
        }
        catch (Exception exception)
        {
            this.logger.LogError($"{Tag}|Unknown exception for: {cachePrefix}", exception);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string cacheKey, string cachePrefix)
    {
        try
        {
            await this.cache.RemoveCachedValueAsync(cacheKey);
        }
        catch (CacheManagerException ex)
        {
            this.logger.LogError($"{Tag}|Error for: {cachePrefix}", ex);
            throw;
        }
        catch (Exception exception)
        {
            this.logger.LogError($"{Tag}|Unknown exception for: {cachePrefix}", exception);
            throw;
        }
    }

    #region IDisposable Support

    /// <summary>
    /// Dispose pattern
    /// </summary>
    /// <param name="disposing">If true, disposing from a regular Dispose call</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // Free unmanaged resources and override a finalizer below.
                if (this.timer != null)
                {
                    this.timer.Dispose();
                    this.timer = null;
                }
            }
            this.disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="CacheManager"/> class.
    /// </summary>
    ~CacheManager()
    {
        this.Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
