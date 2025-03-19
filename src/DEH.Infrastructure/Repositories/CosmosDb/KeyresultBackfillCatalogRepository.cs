namespace DEH.Infrastructure.Repositories;

using Domain.Backfill.Catalog;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;
using Microsoft.Purview.DataGovernance.Catalog.Model;
using System.Collections.Concurrent;

public class KeyresultBackfillCatalogRepository(
    Database cosmosDatabase,
    IDataEstateHealthRequestLogger logger,
    int batchSize = BackfillCatalogRepository.CosmosBatchSize) : BackfillCatalogRepository(cosmosDatabase, "keyresult", logger, batchSize)
{
    private readonly ConcurrentQueue<DataChangeEvent<KeyResult>> _eventQueue = new();

    public override async Task AddBatch<T>(DataChangeEvent<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is not DataChangeEvent<KeyResult> typedEvent)
        {
            throw new InvalidOperationException($"Failed to cast {typeof(T).Name} to KeyResult");
        }

        this._eventQueue.Enqueue(typedEvent);
        this._logger?.LogDebug("Event queued for account {AccountId}. Current queue size: {QueueSize}",
            typedEvent.AccountId, this._eventQueue.Count);

        if (this._eventQueue.Count >= this._batchSize)
        {
            await this.ProcessBatchAsync(this._eventQueue);
        }
    }

    public override async Task FlushAsync()
    {
        await this.ProcessBatchAsync(this._eventQueue, true);
    }
}