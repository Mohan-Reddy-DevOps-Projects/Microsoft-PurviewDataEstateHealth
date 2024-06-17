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
    public async Task<TEntity> AddAsync(TEntity entity, string tenantId, string? accountId = null)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(AddAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}"))
        {
            try
            {
                PopulateMetadataForEntity(entity, tenantId, accountId);
                var tenantPartitionKey = new PartitionKey(tenantId);
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
                    new Context($"{this.GetType().Name}#{methodName}_{entity.Id}_{tenantId}_{accountId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(tenantId, response);
                return response.Resource;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems, IReadOnlyCollection<TEntity> IgnoredItems)> AddAsync(IReadOnlyList<TEntity> entities, string tenantId, string? accountId = null)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(AddAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], [], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, tenantId, accountId);
                }

                var tenantPartitionKey = new PartitionKey(tenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];
                ConcurrentBag<TEntity> ignoredItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.CreateItemAsync(entity, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{tenantId}_{accountId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(responseTask.Result.Resource);
                            cosmosMetricsTracker.LogCosmosMetrics(tenantId, responseTask.Result);
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
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to add entity to CosmosDB in a bulk add, entityType = {entity.GetType().Name}, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems, ignoredItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(TEntity entity, string tenantId)
    {
        ValidateTenantId(tenantId);
        return this.DeleteAsync(entity.Id, tenantId);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, string tenantId)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(DeleteAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, tenantId = {tenantId}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(tenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.DeleteItemAsync<TEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{tenantId}")
                ).ConfigureAwait(false);

                cosmosMetricsTracker.LogCosmosMetrics(tenantId, response);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, tenantId = {tenantId}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task DeleteDEHAsync(string id, string accountId)
    {
        ValidateAccountId(accountId);
        var methodName = nameof(DeleteDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, accountId = {accountId}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(accountId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.DeleteItemAsync<cosmosEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{accountId}")
                ).ConfigureAwait(false);

                cosmosMetricsTracker.LogCosmosMetrics(accountId, response);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, accountId = {accountId}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync(string tenantId)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(GetAllAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, tenantId = {tenantId}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(tenantId);

                var feedIterator = this.CosmosContainer.GetItemLinqQueryable<TEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = tenantPartitionKey }
                ).Where(x => true).ToFeedIterator();

                var results = new List<TEntity>();
                while (feedIterator.HasMoreResults)
                {
                    // Execute the asynchronous operation with the defined retry policy
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => feedIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{tenantId}")
                    ).ConfigureAwait(false);
                    cosmosMetricsTracker.LogCosmosMetrics(tenantId, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, tenantId = {tenantId}", ex);
                throw;
            }
        }
    }

    public class cosmosEntity
    {
        public string Id { get; set; } = "";
        public string accountId { get; set; } = "";

    }


    /// <inheritdoc />
    public async Task<IEnumerable<cosmosEntity>> GetAllDEHAsync(string accountId)
    {
        ValidateAccountId(accountId);
        var methodName = nameof(GetAllDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, accountId = {accountId}"))
        {
            try
            {
                var accountPartitionKey = new PartitionKey(accountId);

                var feedIterator = this.CosmosContainer.GetItemLinqQueryable<cosmosEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = accountPartitionKey }
                ).Where(x => true).ToFeedIterator();

                var results = new List<cosmosEntity>();
                while (feedIterator.HasMoreResults)
                {
                    // Execute the asynchronous operation with the defined retry policy
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => feedIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{accountId}")
                    ).ConfigureAwait(false);
                    cosmosMetricsTracker.LogCosmosMetrics(accountId, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, tenantId = {accountId}", ex);
                throw;
            }
        }
    }


    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, string tenantId)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(GetByIdAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {id}, tenantId = {tenantId}"))
        {
            try
            {
                var tenantPartitionKey = new PartitionKey(tenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.ReadItemAsync<TEntity>(id, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{id}_{tenantId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(tenantId, response);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {id}, tenantId = {tenantId}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, string tenantId, string? accountId = null)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(UpdateAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}"))
        {
            try
            {
                PopulateMetadataForEntity(entity, tenantId, accountId);
                var tenantPartitionKey = new PartitionKey(tenantId);

                // Execute the asynchronous operation with the defined retry policy
                var response = await this.retryPolicy.ExecuteAsync(
                    (context) => this.CosmosContainer.UpsertItemAsync(entity, tenantPartitionKey),
                    new Context($"{this.GetType().Name}#{methodName}_{entity.Id}_{tenantId}_{accountId}")
                ).ConfigureAwait(false);
                cosmosMetricsTracker.LogCosmosMetrics(tenantId, response);
                return response.Resource;
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> UpdateAsync(IReadOnlyList<TEntity> entities, string tenantId, string? accountId = null)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(UpdateAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, tenantId, accountId);
                }

                var tenantPartitionKey = new PartitionKey(tenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.UpsertItemAsync(entity, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{tenantId}_{accountId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(entity);
                            cosmosMetricsTracker.LogCosmosMetrics(tenantId, responseTask.Result);
                        }
                        else
                        {
                            failedItems.Add(entity);
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to update entity to CosmosDB in a bulk update, entityType = {entity.GetType().Name}, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<TEntity> SucceededItems, IReadOnlyCollection<TEntity> FailedItems)> DeleteAsync(IReadOnlyList<TEntity> entities, string tenantId, string? accountId = null)
    {
        var methodName = nameof(DeleteAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}"))
        {
            try
            {
                if (!entities.Any())
                {
                    return ([], []);
                }

                foreach (var entity in entities)
                {
                    PopulateMetadataForEntity(entity, tenantId, accountId);
                }

                var tenantPartitionKey = new PartitionKey(tenantId);

                ConcurrentBag<TEntity> succeededItems = [];
                ConcurrentBag<TEntity> failedItems = [];

                await Task.WhenAll(entities.Select(entity =>
                {
                    // Execute the asynchronous operation with the defined retry policy
                    return this.retryPolicy.ExecuteAsync(
                        (context) => this.CosmosContainer.DeleteItemAsync<TEntity>(entity.Id, tenantPartitionKey),
                        new Context($"{this.GetType().Name}#{methodName}(batch)_{entity.Id}_{tenantId}_{accountId}")
                    ).ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCompletedSuccessfully)
                        {
                            succeededItems.Add(responseTask.Result.Resource);
                            cosmosMetricsTracker.LogCosmosMetrics(tenantId, responseTask.Result);
                        }
                        else
                        {
                            failedItems.Add(entity);
                            logger.LogWarning($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB in a bulk delete, entityType = {entity.GetType().Name}, entityId = {entity.Id}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", responseTask.Exception);
                        }
                    });
                })).ConfigureAwait(false);

                return (succeededItems, failedItems);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, entityCount = {entities.Count}, tenantId = {tenantId}, accountId = {accountId ?? "N/A"}", ex);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeprovisionAsync(string tenantId)
    {
        ValidateTenantId(tenantId);
        var methodName = nameof(DeprovisionAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, tenantId = {tenantId}"))
        {
            try
            {
                // TODO: Delete by partition key feature is in preview in Cosmos SDK.
                // Will switch to that once it's GA. https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example

                var allItems = await this.GetAllAsync(tenantId).ConfigureAwait(false);

                logger.LogInformation($"{this.GetType().Name}#{methodName}, deleting {allItems.Count()} entities from CosmosDB during deprovisioning, tenantId = {tenantId}");

                await Task.WhenAll(allItems.Select(async item =>
                {
                    try
                    {
                        await this.DeleteAsync(item.Id, tenantId).ConfigureAwait(false);

                        logger.LogInformation($"{this.GetType().Name}#{methodName}, deleted entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, tenantId = {tenantId}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, tenantId = {tenantId}", ex);
                    }
                })).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, tenantId = {tenantId}", ex);
            }
        }
    }

    /// <inheritdoc />
    public async Task DeprovisionDEHAsync(string accountId)
    {
        //use this id to Test
        //accountId = "ecf09339-34e0-464b-a8fb-661209048541";

        ValidateAccountId(accountId);
        var methodName = nameof(DeprovisionDEHAsync);
        using (logger.LogElapsed($"{this.GetType().Name}#{methodName}, accountId = {accountId}"))
        {
            try
            {
                // TODO: Delete by partition key feature is in preview in Cosmos SDK.
                // Will switch to that once it's GA. https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example

                var allItems = await this.GetAllDEHAsync(accountId).ConfigureAwait(false);
                var containerName = this.CosmosContainer.Id;

                logger.LogInformation($"{this.GetType().Name}#{methodName}, deleting {allItems.Count()} entities from CosmosDB during deprovisioning, ContainerName = {containerName}, tenantId = {accountId}");

                await Task.WhenAll(allItems.Select(async item =>
                {
                    try
                    {
                        await this.DeleteDEHAsync(item.Id, accountId).ConfigureAwait(false);

                        logger.LogInformation($"{this.GetType().Name}#{methodName}, deleted entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id}, ContainerName = {containerName}, tenantId = {accountId}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{this.GetType().Name}#{methodName}, failed to delete entity from CosmosDB during deprovisioning, entityType = {item.GetType().Name}, entityId = {item.Id},  ContainerName = {containerName}, tenantId = {accountId}", ex);
                    }
                })).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"{this.GetType().Name}#{methodName} failed, tenantId = {accountId}", ex);
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


    private static void PopulateMetadataForEntity(TEntity entity, string tenantId, string? accountId = null)
    {
        ValidateTenantId(tenantId);
        entity.TenantId = tenantId;
        if (accountId != null)
        {
            entity.AccountId = accountId;
        }
    }
}
