namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Local memory repository cache.
/// </summary>
/// <typeparam name="TRepository"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityLocator"></typeparam>
/// <typeparam name="TPayload"></typeparam>
public class LocalMemoryRepositoryCache<TRepository, TEntity, TEntityLocator, TPayload> where TRepository :
    IEntityCreateOperation<TPayload, TEntity>,
    IRetrieveEntityByIdOperation<TEntityLocator, TEntity>,
    IEntityDeleteOperation<TEntityLocator>
    where TEntityLocator : LocatorEntityBase
    where TEntity : IEntityModel
    where TPayload : class
{
    private readonly ICacheManager cacheManager;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly string cachePrefix;
    private readonly TimeSpan cacheTTLDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMemoryRepositoryCache{TReprository, TEntity, TEntityLocator, TPayload}"/> class.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="cacheTTLDuration"></param>
    /// <param name="cacheManager"></param>
    /// <param name="logger"></param>
    public LocalMemoryRepositoryCache(TRepository repository, TimeSpan cacheTTLDuration, ICacheManager cacheManager, IDataEstateHealthRequestLogger logger)
    {
        this.Repository = repository;
        this.cacheTTLDuration = cacheTTLDuration;
        this.cacheManager = cacheManager;
        this.logger = logger;

        this.cachePrefix = typeof(TRepository).FullName;
    }

    /// <summary>
    /// Gets the repository.
    /// </summary>
    protected TRepository Repository { get; private set; }

    /// <summary>
    /// Deletes the entity.
    /// </summary>
    /// <param name="entityLocator"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<DeletionResult> Delete(TEntityLocator entityLocator, CancellationToken cancellationToken)
    {
        string key = this.GetEntityLookupKey(entityLocator);
        DeletionResult response = await this.Repository.Delete(entityLocator, cancellationToken);
        await this.cacheManager.RemoveAsync(key, this.cachePrefix);

        return response;
    }

    /// <summary>
    /// Gets the entity lookup key.
    /// </summary>
    /// <param name="entityLocator"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> Get(TEntityLocator entityLocator, CancellationToken cancellationToken)
    {
        string key = this.GetEntityLookupKey(entityLocator);
        TEntity entity = await this.cacheManager.GetAsync<TEntity>(key, this.cachePrefix);
        if (entity == null)
        {
            entity = await this.Repository.Get(entityLocator, cancellationToken);
            if (entity != null)
            {
                await this.cacheManager.SetAsync(key, entity, this.cacheTTLDuration, this.cachePrefix);
            }
        }

        return entity;
    }

    /// <summary>
    /// Adds or updates the entity.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> Create(TPayload payload, CancellationToken cancellationToken)
    {
        var entity = await this.Repository.Create(payload, cancellationToken);

        try
        {
            string key = this.GetEntityLookupKey(entity);
            await this.cacheManager.RemoveAsync(key, this.cachePrefix);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning("Failed to invalidate cache after updating the database. This can lead to an inconsistent state.", ex);
        }

        return entity;
    }

    private string GetEntityLookupKey(TEntity entity) => $"{this.cachePrefix}-{entity.PartitionKey}-{entity.RowKey}";

    private string GetEntityLookupKey(TEntityLocator entityLocator) => $"{this.cachePrefix}-{entityLocator.PartitionKey}-{entityLocator.ResourceId()}";
}