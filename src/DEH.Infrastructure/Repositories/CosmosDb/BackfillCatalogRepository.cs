namespace DEH.Infrastructure.Repositories;

using Domain.Backfill.Catalog;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;

public abstract class BackfillCatalogRepository(
    Database cosmosDatabase,
    string containerName,
    IDataEstateHealthRequestLogger logger,
    int batchSize = BackfillCatalogRepository.CosmosBatchSize)
    : IBackfillCatalogRepository
{
    private readonly Container _cosmosContainer = cosmosDatabase.GetContainer(containerName);
    protected readonly IDataEstateHealthRequestLogger _logger = logger;
    private readonly SemaphoreSlim _batchLock = new(1, 1);
    protected const int CosmosBatchSize = 100;
    protected readonly int _batchSize = batchSize;
    private bool _isProcessing;

    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver(),
        Converters = new List<JsonConverter> { new StringEnumConverter() }
    };

    public abstract Task AddBatch<T>(DataChangeEvent<T> value) where T : class;

    public abstract Task FlushAsync();

    protected async Task ProcessBatchAsync<T>(ConcurrentQueue<DataChangeEvent<T>> eventQueue, bool flushAll = false) where T : class
    {
        await this._batchLock.WaitAsync();

        try
        {
            if (this._isProcessing || eventQueue.IsEmpty)
            {
                return;
            }

            this._isProcessing = true;
            this._logger.LogInformation("Starting batch processing with {Count} events in queue", eventQueue.Count);

            int itemsToProcess = flushAll ? eventQueue.Count : Math.Min(this._batchSize, eventQueue.Count);
            var batchItems = new List<DataChangeEvent<T>>();

            for (int i = 0; i < itemsToProcess; i++)
            {
                if (eventQueue.TryDequeue(out var item))
                {
                    batchItems.Add(item);
                }
                else
                {
                    break;
                }
            }

            if (batchItems.Count == 0)
            {
                return;
            }

            var groupedByPartitionKey = batchItems.GroupBy(item => item.AccountId);

            foreach (var group in groupedByPartitionKey)
            {
                string partitionKey = group.Key;
                var itemsInGroup = group.ToList();

                for (int i = 0; i < itemsInGroup.Count; i += CosmosBatchSize)
                {
                    var subBatch = itemsInGroup.Skip(i).Take(CosmosBatchSize).ToList();
                    await this.ProcessBatchGroupAsync(partitionKey, subBatch);
                }
            }

            this._logger.LogInformation("Batch processing completed for {Count} events", batchItems.Count);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error processing batch: {ErrorMessage}", ex.Message);
            throw;
        }
        finally
        {
            this._isProcessing = false;
            this._batchLock.Release();

            if (eventQueue.Count >= this._batchSize)
            {
                await this.ProcessBatchAsync(eventQueue);
            }
        }
    }

    private async Task ProcessBatchGroupAsync<T>(string accountId, List<DataChangeEvent<T>> items) where T : class
    {
        const int maxRetries = 3;
        const int baseDelayMs = 1000;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var batch = this._cosmosContainer.CreateTransactionalBatch(new PartitionKey(accountId));

                foreach (object? itemAsObject in items.Select(item => JsonConvert.SerializeObject(item, this._serializerSettings))
                             .Select(JsonConvert.DeserializeObject<object>))
                {
                    batch.UpsertItem(itemAsObject, new TransactionalBatchItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false
                    });
                }

                using var response = await batch.ExecuteAsync();

                if (!response.IsSuccessStatusCode)
                {
                    this._logger?.LogError(
                        "Batch operation failed with status code {StatusCode} on attempt {Attempt}/{MaxRetries} for account {AccountId}",
                        response.StatusCode, attempt + 1, maxRetries, accountId);

                    for (int i = 0; i < response.Count; i++)
                    {
                        if (!response[i].IsSuccessStatusCode)
                        {
                            this._logger?.LogError("Item {Index} failed with status {StatusCode} for account {AccountId}",
                                i, response[i].StatusCode, accountId);
                        }
                    }
                    if (attempt == maxRetries)
                    {
                        throw new InvalidOperationException($"Batch operation failed with status code {response.StatusCode} after {maxRetries} attempts");
                    }

                    // Calculate delay with exponential backoff (2^attempt * baseDelay)
                    int delayMs = baseDelayMs * (int)Math.Pow(2, attempt);
                    this._logger?.LogInformation(
                        "Retrying batch operation in {DelayMs}ms (attempt {NextAttempt}/{MaxRetries}) for account {AccountId}",
                        delayMs, attempt + 2, maxRetries, accountId);
                    
                    await Task.Delay(delayMs).ConfigureAwait(false);
                    continue;
                }

                this._logger?.LogInformation(
                    "Successfully processed batch of {Count} items for account {AccountId}. Request charge: {RequestCharge} RUs",
                    items.Count, accountId, response.RequestCharge);
                
                return; // Success - exit the retry loop
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                // Calculate delay with exponential backoff
                int delayMs = baseDelayMs * (int)Math.Pow(2, attempt);
                this._logger?.LogError(ex,
                    "Error processing batch group for account {AccountId} on attempt {Attempt}/{MaxRetries}. Retrying in {DelayMs}ms: {ErrorMessage}",
                    accountId, attempt + 1, maxRetries, delayMs, ex.Message);
                
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Final attempt failed
                this._logger?.LogError(ex,
                    "Error processing batch group for account {AccountId} after {MaxRetries} attempts: {ErrorMessage}",
                    accountId, maxRetries, ex.Message);
                throw;
            }
        }
    }
}