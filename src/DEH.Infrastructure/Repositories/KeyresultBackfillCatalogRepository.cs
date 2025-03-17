namespace DEH.Infrastructure.Repositories;

using Domain.Backfill.Catalog;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;
using Microsoft.Purview.DataGovernance.Catalog.Model;
using System.Collections.Concurrent;

public class KeyresultBackfillCatalogRepository : BackfillCatalogRepository, IBackfillCatalogRepository
{
    private readonly string _containerName = "keyresult";
    private readonly Container _cosmosContainer;
    private readonly IDataEstateHealthRequestLogger _logger;
    private readonly ConcurrentQueue<DataChangeEvent<KeyResult>> _eventQueue;
    private readonly SemaphoreSlim _batchLock = new(1, 1);
    private const int _cosmosBatchSize = 100;

    private readonly int _batchSize;
    private bool _isProcessing;

    public KeyresultBackfillCatalogRepository(
        Database cosmosDatabase,
        IDataEstateHealthRequestLogger logger,
        int batchSize = _cosmosBatchSize) : base(cosmosDatabase)
    {
        this._cosmosContainer = cosmosDatabase.GetContainer(this._containerName);
        this._logger = logger;
        this._batchSize = batchSize;
        this._eventQueue = new ConcurrentQueue<DataChangeEvent<KeyResult>>();
        this._isProcessing = false;
    }

    public async Task AddBatch<T>(DataChangeEvent<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        // We're casting here because we know T is KeyResult
        if (value is not DataChangeEvent<KeyResult> typedEvent)
        {
            throw new InvalidOperationException($"Failed to cast {typeof(T).Name} to KeyResult");
        }

        // Add to queue
        this._eventQueue.Enqueue(typedEvent);
        this._logger?.LogDebug("Event queued for account {AccountId}. Current queue size: {QueueSize}",
            typedEvent.AccountId, this._eventQueue.Count);

        // Process batch if we've reached the threshold
        if (this._eventQueue.Count >= this._batchSize)
        {
            await this.ProcessBatchAsync();
        }
    }

    public async Task FlushAsync()
    {
        await this.ProcessBatchAsync(true);
    }

    private async Task ProcessBatchAsync(bool flushAll = false)
    {
        // Prevent concurrent batch operations
        await this._batchLock.WaitAsync();

        try
        {
            // Check if already processing or if the queue is empty
            if (this._isProcessing || this._eventQueue.IsEmpty)
            {
                return;
            }

            this._isProcessing = true;
            this._logger?.LogInformation("Starting batch processing with {Count} events in queue", this._eventQueue.Count);

            // Determine how many items to process
            int itemsToProcess = flushAll
                ? this._eventQueue.Count
                : Math.Min(this._batchSize, this._eventQueue.Count);

            // Dequeue items for processing
            var batchItems = new List<DataChangeEvent<KeyResult>>();
            for (int i = 0; i < itemsToProcess; i++)
            {
                if (this._eventQueue.TryDequeue(out var item))
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

            // Group by AccountId (which is the partition key)
            var groupedByPartitionKey = batchItems.GroupBy(item => item.AccountId);

            // Process each group as a separate batch
            foreach (var group in groupedByPartitionKey)
            {
                string partitionKey = group.Key;
                var itemsInGroup = group.ToList();

                // Process in sub-batches of 100 (Cosmos DB limit)
                for (int i = 0; i < itemsInGroup.Count; i += _cosmosBatchSize)
                {
                    var subBatch = itemsInGroup.Skip(i)
                        .Take(_cosmosBatchSize)
                        .ToList();
                    await this.ProcessBatchGroupAsync(partitionKey, subBatch);
                }
            }

            this._logger?.LogInformation("Batch processing completed for {Count} events", batchItems.Count);
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error processing batch: {ErrorMessage}", ex.Message);
            throw;
        }
        finally
        {
            this._isProcessing = false;
            this._batchLock.Release();

            // Check if we need to process more batches
            if (this._eventQueue.Count >= this._batchSize)
            {
                // Process next batch
                await this.ProcessBatchAsync();
            }
        }
    }

    private async Task ProcessBatchGroupAsync(string accountId, List<DataChangeEvent<KeyResult>> items)
    {
        try
        {
            // Create a transactional batch using accountId as the partition key
            var batch = this._cosmosContainer.CreateTransactionalBatch(
                new PartitionKey(accountId));

            foreach (var item in items)
            {
                batch.UpsertItem(item, new TransactionalBatchItemRequestOptions { EnableContentResponseOnWrite = false });
            }

            // Execute the batch
            using var response = await batch.ExecuteAsync();

            if (!response.IsSuccessStatusCode)
            {
                this._logger?.LogError("Batch operation failed with status code {StatusCode}", response.StatusCode);

                // Handle partial failures
                for (int i = 0; i < response.Count; i++)
                {
                    if (!response[i].IsSuccessStatusCode)
                    {
                        this._logger?.LogError("Item {Index} failed with status {StatusCode} for account {AccountId}",
                            i, response[i].StatusCode, accountId);
                    }
                }

                throw new Exception($"Batch operation failed with status code {response.StatusCode}");
            }

            this._logger?.LogInformation("Successfully processed batch of {Count} items for account {AccountId}. Request charge: {RequestCharge} RUs",
                items.Count, accountId, response.RequestCharge);
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, "Error processing batch group for account {AccountId}: {ErrorMessage}",
                accountId, ex.Message);
            throw;
        }
    }
}