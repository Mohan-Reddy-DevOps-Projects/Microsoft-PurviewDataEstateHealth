namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public abstract class CommonRepository<TEntity>(IDataEstateHealthRequestLogger logger, CosmosMetricsTracker cosmosMetricsTracker) : IRepository<TEntity>
    where TEntity : BaseEntityWrapper, IContainerEntityWrapper
{
    // Define the retry policy
    protected readonly AsyncRetryPolicy retryPolicy = Policy
        // @see: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/conceptual-resilient-sdk-applications#timeouts-and-connectivity-related-failures-http-408503
        .Handle<CosmosException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout || ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Retry 3 times with an exponential backoff
            onRetry: (exception, timespan, retryAttempt, context) =>
            {
                logger.LogWarning($"Retrying#{retryAttempt} due to CosmosException with status code {(exception is CosmosException cosmosException ? cosmosException.StatusCode : "-")}, operation: {context.OperationKey}, timespan: {timespan}", exception);
            });

    protected abstract Container CosmosContainer { get; }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(AddAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {entity.Id}, {accountIdentifier.Log}"))
        {
            try
            {
                PopulateMetadataForEntity(entity, accountIdentifier);
                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);
                var isRetry = false;
                async Task<ItemResponse<TEntity>> Run()
                {
                    if (isRetry)
                    {
                        try
                        {
                            return await this.CosmosContainer.ReadItemAsync<TEntity>(entity.Id, tenantPartitionKey).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.Message, ex);
                        }
                    }
                    isRetry = true;
                    return await this.CosmosContainer.CreateItemAsync(entity, tenantPartitionKey).ConfigureAwait(false);
                }
                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => Run(),
                    new Context($"{this.GetType().Name}#{methodName}_{entity.Id}_{accountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
                return response.Resource;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {entity.Id}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems, IReadOnlyCollection<TEntity> IgnoredItems)> AddAsync(IReadOnlyList<TEntity> entities, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(AddAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, {accountIdentifier.Log}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], [], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, accountIdentifier);
                }

                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];
                ConcurrentBag<TEntity> ignoredItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.CreateItemAsync(entity, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{accountIdentifier.ConcatenatedId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(responseTask.Result.Resource);
                            cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, responseTask.Result);
                        }
                        else
                        {
                            if (responseTask.Exception?.Flatten().InnerExceptions.FirstOrDefault(x => x is CosmosException) is CosmosException cosmosException)
                            {
                                if (cosmosException.StatusCode == HttpStatusCode.Conflict)
                                {
                                    ignoredItems.Add(entity);
                                    return;
                                }
                            }
                            failedItems.Add(entity);
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to add entity to CosmosDB in a bulk add, entityType = {entity.GetType().Name}, entityId = {entity.Id}, {accountIdentifier.Log}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems, ignoredItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(TEntity entity, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        return this.DeleteAsync(entity.Id, accountIdentifier);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(DeleteAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, {accountIdentifier.Log}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.DeleteItemAsync<TEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{accountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);

                cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task DeleteDEHAsync(string id, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(DeleteDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, {accountIdentifier.Log}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(accountIdentifier.AccountId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.DeleteItemAsync<CosmosEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{accountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);

                cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync(AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(GetAllAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, {accountIdentifier.Log}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                var feedIterator = this.CosmosContainer.GetItemLinqQueryable<TEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = tenantPartitionKey }
                ).Where(x => x.AccountId == accountIdentifier.AccountId).ToFeedIterator();

                var results = new List<TEntity>();
                while (feedIterator.HasMoreResults)
                {
                    // Execute the asynchronous operation with the defined retry policy
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => feedIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{accountIdentifier.ConcatenatedId}")
                    ).ConfigureAwait(false);
                    cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    public class CosmosEntity
    {
        public string Id { get; set; } = "";
        public string AccountId { get; set; } = "";

    }


    /// <inheritdoc />
    public async Task<IEnumerable<CosmosEntity>> GetAllDEHAsync(AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(GetAllDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, {accountIdentifier.Log}"))
        {
            try
            {
                var accountPartitionKey = new PartitionKey(accountIdentifier.AccountId);

                var feedIterator = this.CosmosContainer.GetItemLinqQueryable<CosmosEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = accountPartitionKey }
                ).Where(x => true).ToFeedIterator();

                var results = new List<CosmosEntity>();
                while (feedIterator.HasMoreResults)
                {
                    // Execute the asynchronous operation with the defined retry policy
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => feedIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{accountIdentifier.ConcatenatedId}")
                    ).ConfigureAwait(false);
                    cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(GetByIdAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, {accountIdentifier.Log}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.ReadItemAsync<TEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{accountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(UpdateAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {entity.Id}, {accountIdentifier.Log}"))
        {
            try
            {
                PopulateMetadataForEntity(entity, accountIdentifier);
                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.UpsertItemAsync(entity, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{entity.Id}_{accountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, response);
                return response.Resource;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {entity.Id}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> UpdateAsync(IReadOnlyList<TEntity> entities, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(UpdateAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, {accountIdentifier.Log}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, accountIdentifier);
                }

                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.UpsertItemAsync(entity, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{accountIdentifier.ConcatenatedId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(entity);
                            cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, responseTask.Result);
                        }
                        else
                        {
                            failedItems.Add(entity);
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to update entity to CosmosDB in a bulk update, entityType = {entity.GetType().Name}, entityId = {entity.Id}, {accountIdentifier.Log}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> DeleteAsync(IReadOnlyList<TEntity> entities, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(DeleteAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, {accountIdentifier.Log}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, accountIdentifier);
                }

                var tenantPartitionKey = new PartitionKey(accountIdentifier.TenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.DeleteItemAsync<TEntity>(entity.Id, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{accountIdentifier.ConcatenatedId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(responseTask.Result.Resource);
                            cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, responseTask.Result);
                        }
                        else
                        {
                            failedItems.Add(entity);
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB in a bulk delete, entityType = {entity.GetType().Name}, entityId = {entity.Id}, {accountIdentifier.Log}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, {accountIdentifier.Log}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeprovisionAsync(AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(DeprovisionAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, {accountIdentifier.Log}"))
        {
            try
            {
                // TODO: Delete by partition key feature is in preview in Cosmos SDK.
                // Will switch to that once it's GA. https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example

                var allItems = await this.GetAllAsync(accountIdentifier).ConfigureAwait(false);

                logger.LogInformation($"{this.GetType().Name}#{methodName}, deleting {allItems.Count()} entities from CosmosDB during deprovisioning, {accountIdentifier.Log}");

                await Task.WhenAll(allItems.Select(async item =>
                {
                    try
                    {
                        await this.DeleteAsync(item.Id, accountIdentifier).ConfigureAwait(false);

                        logger.LogInformation($"{this.GetType().Name}#{methodName}, deleted entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, {accountIdentifier.Log}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, {accountIdentifier.Log}", ex);
                    }
                })).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, {accountIdentifier.Log}", ex);
            }
        }
    }

    /// <inheritdoc />
    public async Task DeprovisionDEHAsync(AccountIdentifier accountIdentifier)
    {
        //use this id to Test
        //accountId = "ecf09339-34e0-464b-a8fb-661209048541";

        ValidateAccountIdentifier(accountIdentifier);
        var methodName = nameof(DeprovisionDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, {accountIdentifier.Log}"))
        {
            try
            {
                // TODO: Delete by partition key feature is in preview in Cosmos SDK.
                // Will switch to that once it's GA. https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example

                var allItems = await this.GetAllDEHAsync(accountIdentifier).ConfigureAwait(false);
                var containerName = this.CosmosContainer.Id;

                logger.LogInformation($"{this.GetType().Name}#{methodName}, deleting {allItems.Count()} entities from CosmosDB during deprovisioning, ContainerName = {containerName}, {accountIdentifier.Log}");

                await Task.WhenAll(allItems.Select(async item =>
                {
                    try
                    {
                        await this.DeleteDEHAsync(item.Id, accountIdentifier).ConfigureAwait(false);

                        logger.LogInformation($"{this.GetType().Name}#{methodName}, deleted entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, ContainerName = {containerName}, {accountIdentifier.Log}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id},  ContainerName = {containerName}, {accountIdentifier.Log}", ex);
                    }
                })).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, {accountIdentifier.Log}", ex);
            }
        }
    }


    private static void ValidateTenantId(string tenantId)
    {
        // tenantId is a GUID string, it should not be null or all-zero id or invalid GUID string.
        if (string.IsNullOrEmpty(tenantId) || tenantId == Guid.Empty.ToString() || !Guid.TryParse(tenantId, out _))
        {
            throw new ArgumentException($@"Invalid tenantId: ""{tenantId}""", nameof(tenantId));
        }
    }

    private static void ValidateAccountId(string accountId)
    {
        // tenantId is a GUID string, it should not be null or all-zero id or invalid GUID string.
        if (string.IsNullOrEmpty(accountId) || accountId == Guid.Empty.ToString() || !Guid.TryParse(accountId, out _))
        {
            throw new ArgumentException($@"Invalid accountId: ""{accountId}""", nameof(accountId));
        }
    }

    private static void ValidateAccountIdentifier(AccountIdentifier accountIdentifier)
    {
        ValidateTenantId(accountIdentifier.TenantId);
        ValidateAccountId(accountIdentifier.AccountId);
    }


    private static void PopulateMetadataForEntity(TEntity entity, AccountIdentifier accountIdentifier)
    {
        ValidateAccountIdentifier(accountIdentifier);
        entity.TenantId = accountIdentifier.TenantId;
        entity.AccountId = accountIdentifier.AccountId;
    }
}
