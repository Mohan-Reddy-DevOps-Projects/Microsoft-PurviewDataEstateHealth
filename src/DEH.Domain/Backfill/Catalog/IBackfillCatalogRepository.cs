namespace DEH.Domain.Backfill.Catalog;

using Microsoft.Purview.DataGovernance.Catalog.Model;

public interface IBackfillCatalogRepository
{
    Task AddBatch<T>(DataChangeEvent<T> value) where T : class;

    public Task FlushAsync();
}