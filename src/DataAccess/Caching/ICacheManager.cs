namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Caching interface
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// Initialize cache provider, retrieve any required secrets
    /// </summary>
    void Initialize();

    /// <summary>
    /// Tries to retrieve a value from cache
    /// </summary>
    /// <typeparam name="T">Type of the cache item</typeparam>
    /// <param name="cacheKey">Key for caching</param>
    /// <param name="cachePrefix">The grouping of which type of entity is being retrieved.</param>
    /// <returns>Returns an asynchronous operation which returns the cached value. If the value is not found in cache, the async operation returns null</returns>
    Task<T> GetAsync<T>(string cacheKey, string cachePrefix);

    /// <summary>
    /// Stores a value in the cache
    /// </summary>
    /// <typeparam name="T">Type of the cache item</typeparam>
    /// <param name="cacheKey">Key for caching</param>
    /// <param name="value">Value to cache</param>
    /// <param name="duration">Duration to keep item in cache</param>
    /// <param name="cachePrefix">The grouping of which type of entity is being set.</param>
    /// <returns>Returns an asynchronous operation which returns void</returns>
    Task SetAsync<T>(string cacheKey, T value, TimeSpan duration, string cachePrefix);

    /// <summary>
    /// Remove a key and corresponding value from redis cache
    /// </summary>
    /// <param name="cacheKey">the cache key</param>
    /// <param name="cachePrefix">The grouping of which type of entity is being removed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string cacheKey, string cachePrefix);
}