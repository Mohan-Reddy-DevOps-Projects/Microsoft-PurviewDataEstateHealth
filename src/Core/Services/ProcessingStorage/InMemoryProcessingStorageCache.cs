namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using System.Threading;
using System.Threading.Tasks;

internal class InMemoryProcessingStorageCache : IProcessingStorageManager
{
    /// <summary>
    /// Holds information needed to locate a processing storage account in persistence.
    /// </summary>
    private class StorageCacheKey : LocatorEntityBase
    {
        /// <summary>
        /// Initialize storage key
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public StorageCacheKey(Guid accountId) : base(accountId, OwnerNames.Health)
        {
        }

        /// <summary>
        /// Gets the name of the storage account.
        /// </summary>
        /// <returns></returns>
        public override string ResourceId() => ResourceId(ResourceIds.ProcessingStorage, [this.Name.ToString()]);
    }

    private readonly IProcessingStorageManager repository;
    private readonly ICacheManager cacheManager;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly string cachePrefix;
    private readonly TimeSpan cacheTTLDuration;

    public InMemoryProcessingStorageCache(IProcessingStorageManager repository, TimeSpan cacheTTLDuration, ICacheManager cacheManager, IDataEstateHealthRequestLogger logger)
    {
        this.repository = repository;
        this.cacheTTLDuration = cacheTTLDuration;
        this.cacheManager = cacheManager;
        this.logger = logger;

        this.cachePrefix = typeof(IProcessingStorageManager).FullName;
    }

    public async Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken)
    {
        return await this.repository.ConstructContainerPath(containerName, accountId, cancellationToken);
    }

    public async Task<DeletionResult> Delete(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        return await this.repository.Delete(accountServiceModel, cancellationToken);
    }

    public async Task<Models.ProcessingStorageModel> Get(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        return await this.Get(Guid.Parse(accountServiceModel.Id), cancellationToken);
    }

    public async Task<Models.ProcessingStorageModel> Get(Guid accountId, CancellationToken cancellationToken)
    {
        StorageCacheKey entityLocator = new StorageCacheKey(accountId);
        string key = this.GetEntityLookupKey(entityLocator);
        Models.ProcessingStorageModel entity = await this.cacheManager.GetAsync<Models.ProcessingStorageModel>(key, this.cachePrefix);
        if (entity == null)
        {
            entity = await this.repository.Get(accountId, cancellationToken);
            if (entity != null)
            {
                await this.cacheManager.SetAsync(key, entity, this.cacheTTLDuration, this.cachePrefix);
            }
        }

        return entity;
    }

    public async Task<Uri> GetProcessingStorageSasUri(Models.ProcessingStorageModel processingStorageModel, Models.StorageSasRequest parameters, string containerName, CancellationToken cancellationToken)
    {
        return await this.repository.GetProcessingStorageSasUri(processingStorageModel, parameters, containerName, cancellationToken);
    }

    public async Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        await this.repository.Provision(accountServiceModel, cancellationToken);
    }

    public async Task<Stream> GetDataQualityOutput(Models.ProcessingStorageModel processingStorageModel, string folderPath, string fileName)
    {
        return await this.repository.GetDataQualityOutput(processingStorageModel, folderPath, fileName);
    }

    private string GetEntityLookupKey(StorageCacheKey entityLocator) => $"{this.cachePrefix}-{entityLocator.PartitionKey}-{entityLocator.ResourceId()}";
}
